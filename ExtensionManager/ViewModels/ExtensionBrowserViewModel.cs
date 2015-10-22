using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ExtensionManager.Views;
using InRule.Authoring.Windows;

namespace ExtensionManager.ViewModels
{
    using NuGet;

    public class ExtensionBrowserViewModel : IDisposable
    {
        public ExtensionBrowser ExtensionBrowserView { private get; set; }
        public ObservableCollection<IPackage> Extensions { get; }
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }

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

            var remExt = new RemoveExtensionCommand(ExtensionsDirectory, repository);
            remExt.CommandComplete += RemoveExtensionCommandComplete;
            RemoveExtensionCommand = remExt;
        }

        private void RemoveExtensionCommandComplete(object sender, EventArgs eventArgs)
        {
            MessageBox.Show("Extension removed.");
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
            Extensions.Clear();
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

    abstract class CommandBase : ICommand
    {
        protected readonly string InstallPath;
        protected readonly IPackageRepository Repository;
        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);

        public abstract event EventHandler CanExecuteChanged;
        public event EventHandler CommandComplete;

        protected void InvokeCommandComplete()
        {
            CommandComplete?.Invoke(this, EventArgs.Empty);
        }

        protected CommandBase(string extensionPath, IPackageRepository repos)
        {
            Repository = repos;
            InstallPath = extensionPath;
        }
    }

    class RemoveExtensionCommand : CommandBase
    {
        public RemoveExtensionCommand(string extensionPath, IPackageRepository repos) : base(extensionPath, repos) { }
        public override bool CanExecute(object parameter)
        {
            var s = parameter as string;
            var packageList = File.ReadAllLines(ExtensionManager.Constants.PackageListFileName, Encoding.UTF8);
            return s != null && packageList.Contains(s);
        }

        public override void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
        
        public override event EventHandler CanExecuteChanged;
    }

    class AddExtensionCommand : CommandBase
    {
        public AddExtensionCommand(string extensionPath, IPackageRepository repos) : base(extensionPath, repos) {}
        public override bool CanExecute(object parameter)
        {
            var s = parameter as string;
            var packageList = File.ReadAllLines(ExtensionManager.Constants.PackageListFileName, Encoding.UTF8);
            
            return s != null && Repository.Exists(s) && !packageList.Contains(s);
        }

        public override void Execute(object parameter)
        {
            var packageId = parameter as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return;
            }
            
            
            var packageManager = new PackageManager(Repository, Path.Combine(InstallPath, packageId));
            
            try
            {
                packageManager.InstallPackage(packageId);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
            InvokeCommandComplete();
        }

        public override event EventHandler CanExecuteChanged;
    }
}