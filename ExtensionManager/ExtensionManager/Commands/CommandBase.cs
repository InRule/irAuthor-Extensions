using System;
using System.Windows.Input;
using ExtensionManager.ViewModels;
using NuGet;

namespace ExtensionManager.Commands
{
    abstract class CommandBase : ICommand
    {
        protected readonly ExtensionBrowserViewModel ViewModel;
        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);

        public event EventHandler CanExecuteChanged;
        
        protected PackageManager PackageManager => ViewModel.PackageManager;

        protected CommandBase(ExtensionBrowserViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}