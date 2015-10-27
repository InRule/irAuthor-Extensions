using System;
using System.Diagnostics;
using ExtensionManager.ViewModels;
using NuGet;

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

            Repository.RemovePackage(s.Package);

            InvokeCommandComplete(s);
        }
        
        public override event EventHandler CanExecuteChanged;
    }
}