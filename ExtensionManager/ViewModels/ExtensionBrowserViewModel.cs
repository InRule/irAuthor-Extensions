using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ExtensionManager.ViewModels
{
    using NuGet;

    public class ExtensionBrowserViewModel : IDisposable
    {
        public ObservableCollection<IPackage> Extensions { get; private set; }
        public ICommand AddExtensionCommand { get; private set; }

        private const string RoadGetFeedUrl = "http://roadget.azurewebsites.net/nuget"; // TODO: move this into a runtime-configurable setting
        private const string ExtensionsDirectory = @"C:\Program Files (x86)\InRule\IrAuthor\Extensions\Extension Manager";

        private readonly IPackageRepository repository;

        public ExtensionBrowserViewModel()
        {
            repository = PackageRepositoryFactory.Default.CreateRepository(RoadGetFeedUrl);
            var packages = repository.GetPackages().Where(x => x.Tags.Contains("extension")).ToList();
            Extensions = new ObservableCollection<IPackage>(packages);

            AddExtensionCommand = new AddExtensionCommand(RoadGetFeedUrl, ExtensionsDirectory, repository);
        }

        private void AddExtensionCommandOnCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //if (AddExtensionCommand != null)
            //{
            //    AddExtensionCommand.CanExecuteChanged -= AddExtensionCommandOnCanExecuteChanged;
            //}
        }
    }

    class AddExtensionCommand : ICommand
    {
        private readonly IPackageRepository repository;
        
        private readonly string installPath;
        public AddExtensionCommand(string feedUrl, string extensionPath, IPackageRepository repos)
        {
            repository = repos;
            installPath = extensionPath;

        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var packageId = parameter as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return;
            }
            var packageManager = new PackageManager(repository, installPath);
            packageManager.InstallPackage(packageId);

        }

        public event EventHandler CanExecuteChanged;
    }
}