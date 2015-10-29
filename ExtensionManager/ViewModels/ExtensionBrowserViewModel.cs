using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ExtensionManager.Commands;
using ExtensionManager.Views;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;

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
            Debug.WriteLine(string.Format("{0}: {1}", level, string.Format(message, parameters)));
        }

        public FileConflictResolution ResolveFileConflict(string conflict)
        {
            Log(MessageLevel.Error, conflict);
            return FileConflictResolution.Ignore;
        }
    }

    public class ExtensionRowViewModel : INotifyPropertyChanged
    {
        public IPackage Package { get; set; }

        public IPackageMetadata PackageMetadata {  get { return Package as IPackageMetadata; } }

        public bool IsInstalled
        {
            get { return isInstalled; }
            set
            {
                var shouldRaise = isInstalled == value;
                isInstalled = value;
                if (PropertyChanged != null && shouldRaise) {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsInstalled"));
                }
            }
        }
        private bool isInstalled;
        public bool IsEnabled {
            get { return isEnabled; }
            set
            {
                var shouldRaise = isEnabled == value;
                isEnabled = value;
                if (PropertyChanged != null && shouldRaise)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
        }
        private bool isEnabled;
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ExtensionBrowserViewModel : IDisposable
    {
        public event EventHandler<ExtensionManagerSettings> SettingsChanged;

        public ExtensionBrowser ExtensionBrowserView { private get; set; }
        public ObservableCollection<ExtensionRowViewModel> Extensions { get; } 
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }

        private const string RoadGetFeedUrl = "http://roadget.azurewebsites.net/nuget"; // TODO: move this into a runtime-configurable setting
        private readonly string ExtensionsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"InRule\irAuthor\ExtensionExchange");

        private readonly IPackageRepository repository;
        private ExtensionManagerSettings settings;

        public ExtensionBrowserViewModel() : this(new ExtensionManagerSettings())
        {
            
        }

        public ExtensionBrowserViewModel(ExtensionManagerSettings settings)
        {
            this.settings = settings;
            Extensions = new ObservableCollection<ExtensionRowViewModel>();
            repository = PackageRepositoryFactory.Default.CreateRepository(RoadGetFeedUrl);
            RefreshPackageList();

            var addExt = new AddExtensionCommand(ExtensionsDirectory, repository);
            addExt.CommandComplete += AddExtensionComplete;
            AddExtensionCommand = addExt;

            var remExt = new RemoveExtensionCommand(ExtensionsDirectory, repository);
            remExt.CommandComplete += RemoveExtensionCommandComplete;
            RemoveExtensionCommand = remExt;
        }

        private void RemoveExtensionCommandComplete(object sender, ExtensionCommandEventArgs eventArgs)
        {
            var extension = eventArgs.Extension;
            extension.IsInstalled = false;
            settings.InstalledExtensions.Remove(extension.Package.Id);
            InvokeSettingsChanged();

            RestartApplicationWithConfirm();
        }

        private void AddExtensionComplete(object sender, ExtensionCommandEventArgs eventArgs)
        {

            var extension = eventArgs.Extension;
            extension.IsInstalled = true;

            settings.InstalledExtensions.Add(extension.Package.Id);
            InvokeSettingsChanged();
            RestartApplicationWithConfirm();
        }

        private void RestartApplicationWithConfirm()
        {
            var result = MessageBox.Show(ExtensionBrowserView,
                       "Requested operation has been completed. IrAuthor must be restarted before your changes can take effect. Would you like to restart IrAuthor now?",
                       "Restart needed", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        public void RefreshPackageList()
        {
            //var packages = repository.GetPackages()
            //    .Where(x => x.Tags.Contains("extension"))
            //    .ToList()
            //    .Join(settings.InstalledExtensions, package => package.Id, s => s,
            //        (package, s) =>
            //            new ExtensionRowViewModel
            //            {
            //                Package = package,
            //                IsInstalled = !string.IsNullOrWhiteSpace(s),
            //                IsEnabled = false
            //            })
            //    .ToList();
            var packages = repository.GetPackages().Where(x => x.Tags.Contains("extension")).ToList()
                .Select(pkg => new ExtensionRowViewModel {Package = pkg, IsInstalled = settings.InstalledExtensions.Contains(pkg.Id)});

            Extensions.Clear();
            Extensions.AddRange(packages);
        }

        private void InvokeSettingsChanged()
        {
            if (SettingsChanged != null)
            {
                SettingsChanged(this, settings);
            }
        }
        public void Dispose()
        {
            var addCommand = AddExtensionCommand as AddExtensionCommand;
            if (addCommand != null)
            {
                addCommand.CommandComplete -= AddExtensionComplete;
            }
            var removeCommand = RemoveExtensionCommand as RemoveExtensionCommand;
            if (removeCommand != null)
            {
                removeCommand.CommandComplete -= RemoveExtensionCommandComplete;
            }

        }
    }
}