using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using ExtensionManager.ViewModels;
using InRule.Authoring.Windows;

namespace ExtensionManager.Commands
{
    class AddExtensionCommand : CommandBase
    {
        public AddExtensionCommand(ExtensionBrowserViewModel viewModel) 
            : base(viewModel)
        {}

        public override bool CanExecute(object parameter)
        {
            var vm = parameter as ExtensionRowViewModel;
            return vm?.Package != null && !vm.IsInstalled;
        }

        public override void Execute(object parameter)
        {
            var vm = parameter as ExtensionRowViewModel;

            if (vm == null) return;

            var window = new BackgroundWorkerWaitWindow("Install Extension", $"Installing the '{vm.PackageMetadata.Title}' extension...");
            window.DoWork += delegate
            {
                PackageManager.InstallPackage(vm.Package, false, true);
            };
            window.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args)
            {
                if (args.Error == null)
                {
                    vm.IsInstalled = true;

                    if (!ViewModel.Settings.EnabledExtensions.Contains(vm.ExtensionId.ToString()))
                    {
                        ViewModel.Settings.EnabledExtensions.Add(vm.ExtensionId.ToString());
                    }

                    ViewModel.InvokeSettingsChanged();
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