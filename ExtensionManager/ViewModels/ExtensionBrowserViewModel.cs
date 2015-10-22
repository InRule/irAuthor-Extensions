using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ExtensionManager.Commands;
using ExtensionManager.Views;
using InRule.Authoring.Windows;

namespace ExtensionManager.ViewModels
{
    using NuGet;

    public class ExtensionBrowserViewModel : IDisposable
    {
        public ExtensionBrowser ExtensionBrowserView { private get; set; }
        public ObservableCollection<IPackage> Extensions { get; }

        public ObservableCollection<string> InstalledExtensions { get; } 
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }

        private const string RoadGetFeedUrl = "http://roadget.azurewebsites.net/nuget"; // TODO: move this into a runtime-configurable setting
        private const string ExtensionsDirectory = @"C:\Program Files (x86)\InRule\IrAuthor\Extensions\Extension Manager";

        private readonly IPackageRepository repository;
        private ExtensionManagerSettings settings;

        public ExtensionBrowserViewModel() : this(new ExtensionManagerSettings())
        {
            Extensions = new ObservableCollection<IPackage>();
            repository = PackageRepositoryFactory.Default.CreateRepository(RoadGetFeedUrl);
            RefreshPackageList();

            var addExt = new AddExtensionCommand(ExtensionsDirectory, repository);
            addExt.CommandComplete +=AddExtensionComplete;
            AddExtensionCommand = addExt;

            var remExt = new RemoveExtensionCommand(ExtensionsDirectory, repository);
            remExt.CommandComplete += RemoveExtensionCommandComplete;
            RemoveExtensionCommand = remExt;
        }

        public ExtensionBrowserViewModel(ExtensionManagerSettings settings)
        {
            this.settings = settings;
            InstalledExtensions = new ObservableCollection<string>(settings.InstalledExtensions);
        }

        private void RemoveExtensionCommandComplete(object sender, ExtensionCommandEventArgs eventArgs)
        {
            MessageBox.Show("Extension removed.");
        }

        private void AddExtensionComplete(object sender, ExtensionCommandEventArgs eventArgs)
        {
            if (!string.IsNullOrWhiteSpace(eventArgs.ExtensionId))
            {
                InstalledExtensions.Add(eventArgs.ExtensionId);
            }

            var result = MessageBox.Show(ExtensionBrowserView,
                       "Extension installed. IrAuthor must be restarted before you can use your new extension. Would you like to close IrAuthor now?",
                       "Restart needed", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Environment.Exit(0);
            }
        }

        public void RefreshPackageList()
        {
            var packages = repository.GetPackages().Where(x => x.Tags.Contains("extension")).ToList();
            Extensions.Clear();
            Extensions.AddRange(packages);
        }

        public void Dispose()
        {
            var command = AddExtensionCommand as AddExtensionCommand;
            if (command != null)
            {
                command.CommandComplete -= AddExtensionComplete;
            }
        }
    }
}