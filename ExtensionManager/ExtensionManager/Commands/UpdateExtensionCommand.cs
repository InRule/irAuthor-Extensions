using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ExtensionManager.ViewModels;
using NuGet;

namespace ExtensionManager.Commands
{
    class UpdateExtensionCommand : CommandBase
    {
        public UpdateExtensionCommand(string extensionsDirectory, IPackageRepository repository, ExtensionBrowserViewModel extensionBrowserViewModel)
            : base(extensionsDirectory, repository, extensionBrowserViewModel)
        {

        }

        public override event EventHandler CanExecuteChanged;

        public override bool CanExecute(object parameter)
        {
            var s = parameter as ExtensionRowViewModel;
            var canExecute = s != null && s.UpdateAvailable;
            
            return canExecute;
        }

        public override void Execute(object parameter)
        {
            var rowModel = parameter as ExtensionRowViewModel;
            if (rowModel == null)
            {
                return;
            }
            ViewModel.RaiseWorkStarted();
            var dispatcher = Dispatcher.CurrentDispatcher;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    PackageManager.UpdatePackage(rowModel.Package.Id, false, true);
                    rowModel.UpdateAvailable = false;
                    dispatcher.BeginInvoke(new Action(() => ViewModel.RestartApplicationWithConfirm()));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show(ex.ToString());
                    throw;
                }
            }).ContinueWith((t) => ViewModel.RaiseWorkComplete(), TaskScheduler.FromCurrentSynchronizationContext());

        }
    }
}