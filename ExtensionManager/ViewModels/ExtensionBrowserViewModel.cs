using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ExtensionManager.Commands;
using ExtensionManager.Views;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Settings;

namespace ExtensionManager.ViewModels
{
    using NuGet;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;

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

        public ICommand UpdateExtensionCommand
        {
            get;
        }
        public IEnumerable<IExtension> InstalledExtensions { get; set; } 
        public readonly PackageManager PackageManager;

        private readonly string ExtensionsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"InRule\irAuthor\ExtensionExchange");
        private readonly AggregateRepository repository;

        internal readonly ExtensionManagerSettings Settings;
        private int operationProgress = 0;

        public ExtensionBrowserViewModel() : this(new ExtensionManagerSettings()) {}

        public ExtensionBrowserViewModel(ExtensionManagerSettings settings)
        {
            Settings = settings;

            Extensions = new ObservableCollection<ExtensionRowViewModel>();
            InstalledExtensions = new List<IExtension>();

            repository = new AggregateRepository(PackageRepositoryFactory.Default, new[] {
                "http://roadget.azurewebsites.net/nuget/",
                "https://www.nuget.org/api/v2/curated-feeds/microsoftdotnet/",
                "https://api.nuget.org/v3/index.json",
                }, true);

            repository.ResolveDependenciesVertically = true;
            PackageManager = new PackageManager(repository, ExtensionsDirectory)
            {
                Logger = new DebugLogger()
            };
            PackageManager.FileSystem.Logger = PackageManager.Logger;
            repository.Logger = PackageManager.Logger;

            var addExt = new AddExtensionCommand(ExtensionsDirectory, repository, this);
            AddExtensionCommand = addExt;

            var remExt = new RemoveExtensionCommand(ExtensionsDirectory, repository, this);
            RemoveExtensionCommand = remExt;

            var updateExt = new UpdateExtensionCommand(ExtensionsDirectory, repository, this);
            UpdateExtensionCommand = updateExt;

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

        public void RefreshPackageList(bool showInstalledOnly = false)
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            RaiseWorkStarted();
            
            Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("In refresh packages Task");
                
                var packages = repository.GetPackages()
                    .Where(x => x.Tags.Contains("extension"))
                    .ToList()
                    .GroupBy(x => x.Id, (id, packs) =>
                    {
                        SemanticVersion latestVersion;
                        latestVersion = packs.Max(v => v.Version);
                        IPackage currentPackage = PackageManager.LocalRepository.FindPackage(id);
                        return new ExtensionRowViewModel
                        {
                            UpdateAvailable = currentPackage != null && !currentPackage.IsLatestVersion,
                            IsInstalled = currentPackage != null,
                            LatestVersion = latestVersion.ToNormalizedString(),
                            InstalledVersion = currentPackage == null ? "--" : currentPackage.Version.ToNormalizedString(),                            
                            Package = packs.First(p => p.Id == id)
                        };
                    })
                    .ToList()
                    .Where(x => !showInstalledOnly || x.IsInstalled);
                Debug.WriteLine("Got packages. Invoking action on dispatcher to populate UI");
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