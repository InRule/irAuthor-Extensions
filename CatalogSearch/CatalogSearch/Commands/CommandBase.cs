using System;
using System.Windows.Input;
using System.ComponentModel;
using CatalogSearch.ViewModels;

namespace CatalogSearch.Commands
{
    abstract class CommandBase : ICommand
    {
        protected readonly CatalogSearchViewModel ViewModel;
        protected readonly BackgroundWorker Worker;
        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);

        public abstract event EventHandler CanExecuteChanged;

        protected CommandBase(CatalogSearchViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}