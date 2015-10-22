using System;
using NuGet;

namespace ExtensionManager.Commands
{
    class RemoveExtensionCommand : CommandBase
    {
        public RemoveExtensionCommand(string extensionPath, IPackageRepository repos) : base(extensionPath, repos) { }
        public override bool CanExecute(object parameter)
        {
            var s = parameter as string;
            
            return s != null ;
        }

        public override void Execute(object parameter)
        {
            var s = parameter as string;
            InvokeCommandComplete(s);
        }
        
        public override event EventHandler CanExecuteChanged;
    }
}