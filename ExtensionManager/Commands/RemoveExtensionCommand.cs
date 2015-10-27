using System;
using System.Diagnostics;
using ExtensionManager.ViewModels;
using NuGet;
using System.Windows;
using System.IO;

namespace ExtensionManager.Commands
{
    class RemoveExtensionCommand : CommandBase
    {
        public RemoveExtensionCommand(string extensionPath, IPackageRepository repos) : base(extensionPath, repos) { }
        public override bool CanExecute(object parameter)
        {
            var s = parameter as ExtensionRowViewModel;
            
            return s != null && s.IsInstalled && s.Package != null;
        }

        public override void Execute(object parameter)
        {
            var s = parameter as ExtensionRowViewModel;
            
            s.IsInstalled = false;
            s.IsEnabled = false;
            var packageManager = new PackageManager(Repository, Path.Combine(InstallPath, s.Package.Id));
            
            try
            {
                packageManager.UninstallPackage(s.Package);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
            InvokeCommandComplete(s);
        }
        
        public override event EventHandler CanExecuteChanged;
    }
}