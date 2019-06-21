using System.ComponentModel;
using System.Diagnostics;
using ExtensionManager.ViewModels;
using System.Windows;
using InRule.Authoring.Windows;

namespace ExtensionManager.Commands
{
    class RemoveExtensionCommand : CommandBase
    {
        public RemoveExtensionCommand(ExtensionBrowserViewModel viewModel) 
            : base(viewModel)
        {}

        public override bool CanExecute(object parameter)
        {
            var vm = parameter as ExtensionRowViewModel;
            
            return vm != null && vm.IsInstalled && vm.Package != null;
        }

        public override void Execute(object parameter)
        {
            var vm = parameter as ExtensionRowViewModel;
             
            var window = new BackgroundWorkerWaitWindow("Uninstall Extension", $"Uninstalling the '{vm.PackageMetadata.Title}' extension...");
            window.DoWork += delegate
            {
                vm.IsInstalled = false;
                vm.IsEnabled = false;
                PackageManager.UninstallPackage(vm.Package, true, true);
                ViewModel.InvokeSettingsChanged();
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