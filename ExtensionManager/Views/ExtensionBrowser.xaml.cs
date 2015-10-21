using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtensionManager.ViewModels;

namespace ExtensionManager.Views
{
    /// <summary>
    /// Interaction logic for ExtensionBrowser.xaml
    /// </summary>
    public partial class ExtensionBrowser : Window
    {
        private readonly ExtensionBrowserViewModel viewModel;

        public ExtensionBrowser()
        {
            viewModel = new ExtensionBrowserViewModel();
            DataContext = viewModel;

            InitializeComponent();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
