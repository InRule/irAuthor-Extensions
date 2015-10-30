using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ExtensionManager.ViewModels;
using NuGet;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ExtensionManager.Commands
{
    class AddExtensionCommand : CommandBase
    {
        public AddExtensionCommand(string extensionPath, IPackageRepository repos, ExtensionBrowserViewModel viewModel) : base(extensionPath, repos, viewModel) {}
        public override bool CanExecute(object parameter)
        {
            var s = parameter as ExtensionRowViewModel;

            return s?.Package != null && !s.IsInstalled;
        }

        public override void Execute(object parameter)
        {
            var packageVm = parameter as ExtensionRowViewModel;
            if (packageVm == null) return;

            ViewModel.RaiseWorkStarted();
            var dispatcher = Dispatcher.CurrentDispatcher;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    PackageManager.InstallPackage(packageVm.Package, false, true);
                    packageVm.IsInstalled = true;
                    ViewModel.InvokeSettingsChanged();
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

        public override event EventHandler CanExecuteChanged;
    }
}