using CatalogSearch.ViewModels;
using System;

namespace CatalogSearch.ViewModels
{
    class NavigateCommandEventArgs : EventArgs
    {
        public NavigateCommandEventArgs(CatalogSearchResultViewModel result)
        {
            Result = result;
        }

        public CatalogSearchResultViewModel Result { get; set; }
    }
}