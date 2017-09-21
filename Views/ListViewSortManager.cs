using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Views
{

    public class ListViewSortManager
    {
        public ListView lstViewValues { get; private set; }
        private GridViewColumnHeader _listViewSortCol = null;
        private ListViewSortAdorner _listViewListViewSortAdorner = null;
        public Func<string, ListSortDirection> GetColumnDefaultSortDirection;
        public ListViewSortManager(ListView listView)
        {
            this.lstViewValues = listView;
            foreach (var col in SortableColumns)
            {
                col.Click += ColumnHeader_Clicked;
            }
        }

        private void ColumnHeader_Clicked(object sender, RoutedEventArgs e)
        {
            var column = (sender as GridViewColumnHeader);
            this.SortByColumn(column);
        }

        public void ResetSorting()
        {
            if (_listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewListViewSortAdorner);
                lstViewValues.Items.SortDescriptions.Clear();
            }
            _listViewSortCol = null;
        }

        public void SortByColumn(GridViewColumnHeader column)
        {
            if (column == null)
            {
                return;
            }
            if (_listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewListViewSortAdorner);
                lstViewValues.Items.SortDescriptions.Clear();
            }

            string softColumn = column.Tag.ToString();
            ListSortDirection sortDirection = ListSortDirection.Ascending;

            if (GetColumnDefaultSortDirection != null)
            {
                sortDirection = GetColumnDefaultSortDirection(softColumn);
            }

            if (_listViewSortCol == column && _listViewListViewSortAdorner.Direction == sortDirection)
            {
                sortDirection = ListSortDirection.Descending;
            }

            _listViewSortCol = column;
            _listViewListViewSortAdorner = new ListViewSortAdorner(_listViewSortCol, sortDirection);
            AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewListViewSortAdorner);
            lstViewValues.Items.SortDescriptions.Add(new SortDescription(softColumn, sortDirection));
        }

      
        public void SortByColumnNameAscending(string columnName)
        {
            ResetSorting();
            var col = SortableColumns.FirstOrDefault(c => c.Tag.ToString() == columnName);

            if (col != null)
            {
                SortByColumn(col);
            }
        }

        private List<GridViewColumnHeader> SortableColumns
        {
            get
            {
                var columnsWithTags =
                    (this.lstViewValues.View as GridView).Columns.Select(c => c.Header)
                        .OfType<GridViewColumnHeader>().Where(c => c.Tag is string)
                        .ToList();
                return columnsWithTags;
            }
        }

        public bool HasSortApplied { get { return this._listViewSortCol != null; } }
    }

    public class ListViewSortAdorner : Adorner
    {
        public static double ArrowHeight = 2.5;
        public static double ArrowWidth = 7;
        public static double ArrowWidthOver2 = (ArrowWidth)/2d;
        private static Geometry _ascendingArrowGeometry = Geometry.Parse($"M 0 {ArrowHeight} L {ArrowWidthOver2} 0 L {ArrowWidth} {ArrowHeight}");
        private static Geometry _descendingArrowGeometry = Geometry.Parse($"M 0 0 L {ArrowWidthOver2} {ArrowHeight} L {ArrowWidth} 0");
        private static Pen _DrawingPen;

        public ListSortDirection Direction { get; private set; }
        
        public ListViewSortAdorner(UIElement element, ListSortDirection dir): base(element)
        {
            this.Direction = dir;
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < ArrowWidth+1) return;

            var offsetX = AdornedElement.RenderSize.Width/2 - ArrowWidthOver2;

            double offsetY;
            Geometry geometry;
            
            if (this.Direction == ListSortDirection.Descending)
            {
                geometry = _descendingArrowGeometry;
                offsetY = 1d;
            }
            else
            {
                geometry = _ascendingArrowGeometry;
                offsetY = 1.5d;
            }
            var transform = new TranslateTransform(offsetX, offsetY);
            drawingContext.PushTransform(transform);
            drawingContext.DrawGeometry(null, DrawingPen, geometry);
            drawingContext.Pop();
        }

       

        private static Pen DrawingPen
        {
            get
            {
                if (_DrawingPen == null)
                {
                    _DrawingPen = new Pen(new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)), .9);
                    _DrawingPen.Freeze();
                }
                return _DrawingPen;
            }
        }
    }
    
    public static class ListSortExtensions
    {
        public static ListSortDirection Reverse(this ListSortDirection direction)
        {
            if (direction == ListSortDirection.Ascending) return ListSortDirection.Descending;
            return ListSortDirection.Ascending;
        }
    }

}