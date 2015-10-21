using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ExtensionManager.Views;

namespace ExtensionManager.ViewModels
{
    using NuGet;

    public class ExtensionBrowserViewModel : IDisposable
    {
        public ExtensionBrowser ExtensionBrowserView { private get; set; }
        public ObservableCollection<IPackage> Extensions { get; private set; }
        public ICommand AddExtensionCommand { get; private set; }
        public ICommand RemoveExtensionCommand { get; private set; }

        private const string RoadGetFeedUrl = "http://roadget.azurewebsites.net/nuget"; // TODO: move this into a runtime-configurable setting
        private const string ExtensionsDirectory = @"C:\Program Files (x86)\InRule\IrAuthor\Extensions\Extension Manager";

        private readonly IPackageRepository repository;

        public ExtensionBrowserViewModel()
        {
            Extensions = new ObservableCollection<IPackage>();
            repository = PackageRepositoryFactory.Default.CreateRepository(RoadGetFeedUrl);
            RefreshPackageList();

            var addExt = new AddExtensionCommand(ExtensionsDirectory, repository);
            addExt.CommandComplete +=AddExtOnCommandComplete;
                   
            AddExtensionCommand = addExt;
            RemoveExtensionCommand = new RemoveExtensionCommand();

        }

        private void AddExtOnCommandComplete(object sender, EventArgs eventArgs)
        {
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
            Extensions.AddRange(packages);
        }

        public void Dispose()
        {
            var command = AddExtensionCommand as AddExtensionCommand;
            if (command != null)
            {
                command.CommandComplete -= AddExtOnCommandComplete;
            }
        }
    }

    class RemoveExtensionCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;
    }

    class AddExtensionCommand : ICommand
    {
        private readonly IPackageRepository repository;
        
        private readonly string installPath;
        public AddExtensionCommand(string extensionPath, IPackageRepository repos)
        {
            repository = repos;
            installPath = extensionPath;

        }
        public bool CanExecute(object parameter)
        {
            return repository.Exists(parameter as string);
        }

        public void Execute(object parameter)
        {
            var packageId = parameter as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return;
            }
            var packageManager = new PackageManager(repository, Path.Combine(installPath, packageId));
            packageManager.InstallPackage(packageId);

            CommandComplete?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler CommandComplete;
    }
}