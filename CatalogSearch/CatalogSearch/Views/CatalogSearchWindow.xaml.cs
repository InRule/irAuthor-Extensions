using System;
using System.Windows;
using System.Windows.Input;
using CatalogSearch.ViewModels;

namespace CatalogSearch.Views
{
    public partial class CatalogSearchWindow : Window
    {
        private readonly CatalogSearchViewModel viewModel;

        public CatalogSearchWindow(CatalogSearchViewModel vm)
        {
            viewModel = vm;
            viewModel.CatalogSearchView = this;
            DataContext = viewModel;

            InitializeComponent();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Search();
        }

        private void queryStringTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && viewModel.IsSearchEnabled)
            {
                searchButton_Click(sender, e);
            }
        }
    }
}
