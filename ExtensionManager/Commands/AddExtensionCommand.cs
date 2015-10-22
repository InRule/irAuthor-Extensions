using System;
using System.IO;
using System.Windows;
using NuGet;

namespace ExtensionManager.Commands
{
    class AddExtensionCommand : CommandBase
    {
        public AddExtensionCommand(string extensionPath, IPackageRepository repos) : base(extensionPath, repos) {}
        public override bool CanExecute(object parameter)
        {
            var s = parameter as string;

            return s != null && Repository.Exists(s);
        }

        public override void Execute(object parameter)
        {
            var packageId = parameter as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return;
            }

            var packageManager = new PackageManager(Repository, Path.Combine(InstallPath, packageId));
            
            try
            {
                packageManager.InstallPackage(packageId);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
            InvokeCommandComplete(packageId);
        }

        public override event EventHandler CanExecuteChanged;
    }
}