using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using ExtensionManager.ViewModels;
using InRule.Authoring.Windows;

namespace ExtensionManager.Commands
{
    class UpdateExtensionCommand : CommandBase
    {
        public UpdateExtensionCommand(ExtensionBrowserViewModel extensionBrowserViewModel)
            : base(extensionBrowserViewModel)
        {}

        public override bool CanExecute(object parameter)
        {
            var vm = parameter as ExtensionRowViewModel;
            var canExecute = vm != null && vm.UpdateAvailable;
            
            return canExecute;
        }

        public override void Execute(object parameter)
        {
            var vm = parameter as ExtensionRowViewModel;

            var window = new BackgroundWorkerWaitWindow("Update Extension", $"Updating the '{vm.PackageMetadata.Title}' extension...");
            window.DoWork += delegate
            {
                PackageManager.UpdatePackage(vm.Package.Id, false, true);
                vm.UpdateAvailable = false;
            };
            window.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
            {
                if (args.Error == null)
                {
                    ViewModel.RestartApplicationWithConfirm();
                }
                else
                {
                    Debug.WriteLine(args.Error.ToString());
                    MessageBox.Show(args.Error.ToString());
                    throw args.Error;
                }
            };
            window.ShowDialog();
        }
    }
}