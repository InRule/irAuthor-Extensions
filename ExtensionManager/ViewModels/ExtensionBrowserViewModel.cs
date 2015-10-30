using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ExtensionManager.Commands;
using ExtensionManager.Views;

namespace ExtensionManager.ViewModels
{
    using NuGet;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;

    public class DebugLogger : ILogger
    {
        public void Log(MessageLevel level, string message, params object[] parameters)
        {
            Debug.WriteLine($"{level}: {string.Format(message, parameters)}");
        }

        public FileConflictResolution ResolveFileConflict(string conflict)
        {
            Log(MessageLevel.Error, conflict);
            return FileConflictResolution.Ignore;
        }
    }

    public class ExtensionBrowserViewModel : INotifyPropertyChanged, IDisposable
    {
        public event EventHandler<ExtensionManagerSettings> SettingsChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler WorkStarted;
        public event EventHandler WorkComplete;

        public ExtensionBrowser ExtensionBrowserView { private get; set; }
        public ObservableCollection<ExtensionRowViewModel> Extensions { get; }
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }

        public int Progress {
            get { return operationProgress; }
            set {
                if (operationProgress == value) return;
                operationProgress = value;
                Debug.WriteLine("Progress Changing");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
            }
        }

        public bool ShowProgress => Progress > 0;

        private const string RoadGetFeedUrl = "http://roadget.azurewebsites.net/nuget"; // TODO: move this into a runtime-configurable setting
        private readonly string ExtensionsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"InRule\irAuthor\ExtensionExchange");
        private readonly IPackageRepository repository;

        internal readonly ExtensionManagerSettings Settings;
        private int operationProgress = 0;

        public ExtensionBrowserViewModel() : this(new ExtensionManagerSettings()) {}

        public ExtensionBrowserViewModel(ExtensionManagerSettings settings)
        {
            Settings = settings;
            Extensions = new ObservableCollection<ExtensionRowViewModel>();
            repository = PackageRepositoryFactory.Default.CreateRepository(RoadGetFeedUrl);
            

            var addExt = new AddExtensionCommand(ExtensionsDirectory, repository, this);
            AddExtensionCommand = addExt;

            var remExt = new RemoveExtensionCommand(ExtensionsDirectory, repository, this);
            RemoveExtensionCommand = remExt;
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != "Progress") return;
                Debug.WriteLine("ShowProgress changing");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowProgress"));
            };
            
            WorkStarted += (sender, e) => Progress = 1;
            WorkComplete += (sender, e) => Progress = 0;

            RefreshPackageList();
        }

        public void RaiseWorkStarted()
        {
            Debug.WriteLine("Raising WorkStarted");
            WorkStarted?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseWorkComplete()
        {
            Debug.WriteLine("Raising WorkComplete");
            WorkComplete?.Invoke(this, EventArgs.Empty);
        }

        internal void RestartApplicationWithConfirm()
        {
            var result = MessageBox.Show(ExtensionBrowserView,
                       "Requested operation has been completed. IrAuthor must be restarted before your changes can take effect. Would you like to restart IrAuthor now?",
                       "Restart needed", MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes) return;

            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        public void RefreshPackageList()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            RaiseWorkStarted(); 
            Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("In refresh packages Task");
                var packages = repository.GetPackages().Where(x => x.Tags.Contains("extension")).ToList()
                .Select(pkg => new ExtensionRowViewModel { Package = pkg, IsInstalled = Settings.InstalledExtensions.Contains(pkg.Id) });

                dispatcher.BeginInvoke(new Action(() => { Extensions.Clear(); Extensions.AddRange(packages);}));

            }).ContinueWith((t) => RaiseWorkComplete(), TaskScheduler.FromCurrentSynchronizationContext());
        }

        internal void InvokeSettingsChanged()
        {
            SettingsChanged?.Invoke(this, Settings);
        }

        public void Dispose()
        {

        }
    }
}