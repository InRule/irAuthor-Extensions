using System;
using System.IO;
using System.Windows;
using ExtensionManager.ViewModels;
using NuGet;

namespace ExtensionManager.Commands
{
    class AddExtensionCommand : CommandBase
    {
        public AddExtensionCommand(string extensionPath, IPackageRepository repos) : base(extensionPath, repos) {}
        public override bool CanExecute(object parameter)
        {
            var s = parameter as ExtensionRowViewModel;

            return s?.Package != null && !s.IsInstalled;
        }

        public override void Execute(object parameter)
        {
            var packageVm = parameter as ExtensionRowViewModel;
            if (packageVm == null) return;

            var packageManager = new PackageManager(Repository, Path.Combine(InstallPath, packageVm.Package.Id));
            packageManager.Logger = new DebugLogger();
            packageManager.FileSystem.Logger = packageManager.Logger;

            try
            {
                packageManager.InstallPackage(packageVm.Package, false, true);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
            InvokeCommandComplete(packageVm);
        }

        public override event EventHandler CanExecuteChanged;
    }
}