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

            s.IsInstalled = false;
            s.IsEnabled = false;
            var packageManager = new PackageManager(Repository, Path.Combine(InstallPath, s.Package.Id))
            {
                Logger = new DebugLogger()
            };
            packageManager.FileSystem.Logger = packageManager.Logger;
            
            ViewModel.RaiseWorkStarted();
            var dispatcher = Dispatcher.CurrentDispatcher;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    packageManager.UninstallPackage(s.Package, true, true);
                    s.IsInstalled = false;
                    ViewModel.Settings.InstalledExtensions.Remove(s.Package.Id);
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