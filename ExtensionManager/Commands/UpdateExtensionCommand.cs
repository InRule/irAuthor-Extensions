using System;
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
            return s != null && s.IsInstalled && s.UpdateAvailable;

        }

        public override void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}