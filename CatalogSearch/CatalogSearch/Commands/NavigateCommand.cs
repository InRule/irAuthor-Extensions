using System;
using System.Diagnostics;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Threading;
using CatalogSearch.ViewModels;
using System.Linq;

namespace CatalogSearch.Commands
{
    class NavigateCommand : CommandBase
    {
        public NavigateCommand(CatalogSearchViewModel viewModel) : base(viewModel) {}
        public override bool CanExecute(object parameter)
        {
            var result = parameter as CatalogSearchResultViewModel;
            return result.IsInCurrentRuleApp;
        }

        public override void Execute(object parameter)
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var result = parameter as CatalogSearchResultViewModel;

                    dispatcher.BeginInvoke(new Action(() => { ViewModel.PerformNavigate(result.RuleDef ?? result.RuleSetDef); }));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show(ex.ToString());
                    throw;
                }
            });
        }
        public override event EventHandler CanExecuteChanged;
    }
}