using System;
using System.Windows.Input;
using ExtensionManager.ViewModels;
using NuGet;
using System.ComponentModel;

namespace ExtensionManager.Commands
{
    abstract class CommandBase : ICommand
    {
        protected readonly ExtensionBrowserViewModel ViewModel;
        protected readonly BackgroundWorker Worker;
        protected readonly string InstallPath;
        protected readonly IPackageRepository Repository;
        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);

        public abstract event EventHandler CanExecuteChanged;
        
        protected PackageManager PackageManager => ViewModel.PackageManager;

        protected CommandBase(string extensionPath, IPackageRepository repos, ExtensionBrowserViewModel viewModel)
        {
            ViewModel = viewModel;
            Repository = repos;
            InstallPath = extensionPath;

        }
    }
}