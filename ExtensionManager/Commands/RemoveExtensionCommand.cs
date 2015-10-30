using System;
using System.Diagnostics;
using ExtensionManager.ViewModels;
using NuGet;
using System.Windows;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ExtensionManager.Commands
{
    class RemoveExtensionCommand : CommandBase
    {
        public RemoveExtensionCommand(string extensionPath, IPackageRepository repos, ExtensionBrowserViewModel viewModel) : base(extensionPath, repos, viewModel) { }
        public override bool CanExecute(object parameter)
        {
            var s = parameter as ExtensionRowViewModel;
            
            return s != null && s.IsInstalled && s.Package != null;
        }

        public override void Execute(object parameter)
        {
            var s = parameter as ExtensionRowViewModel;
            Debug.Assert(s != null);
             
            ViewModel.RaiseWorkStarted();
            var dispatcher = Dispatcher.CurrentDispatcher;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    s.IsInstalled = false;
                    s.IsEnabled = false;
                    PackageManager.UninstallPackage(s.Package, true, true);
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