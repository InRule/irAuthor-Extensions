using System.Windows;
using ExtensionManager.ViewModels;
using InRule.Authoring.Extensions;

namespace ExtensionManager.Views
{
    public partial class ExtensionBrowser : Window
    {
        public ExtensionBrowser(ExtensionBrowserViewModel vm)
        {
            this.HideMinimizeAndMaximizeButtons();

            vm.ExtensionBrowserView = this;
            DataContext = vm;

            InitializeComponent();

            // Select the first item upon load, if it exists.
            if (extensionsListBox.Items.Count > 0)
            {
                extensionsListBox.SelectedIndex = 0;
            }
        }
    }
}
