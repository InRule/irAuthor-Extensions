using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using InRule.Authoring.Extensions;
using InRule.Repository;
using InRuleLabs.AuthoringExtensions.FieldsInUse.Extensions;
using System.Xml.Linq;
using InRule.Repository.Infos;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Views
{
    /// <summary>
    /// Interaction logic for SelectDBRuleAppPanel.xaml
    /// </summary>
    public partial class FieldDefListView : UserControl, IDisposable
    {
        [Flags]
        public enum Options
        {
            None = 0,
            NameColumn = 1,
            TypeNameColumn = 32,
        }

        public event EventHandler<object> OnCloseView;
        public RuleApplicationDef TargetRuleApplicationDef { get; set; }
        public ListViewSortManager ListViewSortManager { get; private set; }
        public string DefaultSortColumnName { get; set; }
        private List<UnusedFieldAction> _actions;
        private Action<FieldDefListView, IEnumerable<UnusedFieldReference>> _deleteAllAction;
        private bool _hasLoaded;

        public FieldDefListView(RuleApplicationDef ruleAppDef, 
                                Options includedOptions, 
                                List<UnusedFieldAction> actions, 
                                Action<FieldDefListView, IEnumerable<UnusedFieldReference>> deleteAllAction, 
                                string defaultSortColumnName)
        {
            this.Background = Brushes.White;
            _actions = actions;
            _deleteAllAction = deleteAllAction;
            TargetRuleApplicationDef = ruleAppDef;
            DefaultSortColumnName = defaultSortColumnName;
        
            DataContext = this;
            _hasLoaded = false;
            this.Initialized += FieldDefListView_Initialized;
            InitializeComponent();

            DataTemplate cardLayout = new DataTemplate();
            
            if (includedOptions.HasFlag(Options.NameColumn)) { AddColumn("NameColumn");}
            if (includedOptions.HasFlag(Options.TypeNameColumn)) { AddColumn("TypeNameColumn"); }

            actions = new List<UnusedFieldAction>(actions);
            
            FrameworkElementFactory spFactory = new FrameworkElementFactory(typeof(StackPanel));
            spFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            int count = 0;
            foreach (var action in actions)
            {
                action.ListView = this;
                if (count > 0)
                {
                    var borderFactory = new FrameworkElementFactory(typeof(Border));
                    borderFactory.SetValue(Button.BorderThicknessProperty, new Thickness(1,0,0,0));
                    borderFactory.SetValue(Button.BorderBrushProperty, Brushes.DarkGray);
                    //borderFactory.SetValue(Button.PaddingProperty, new Thickness(5,3,5,3));
                    borderFactory.SetValue(Button.MarginProperty, new Thickness(5, 5, 5, 5));
                    spFactory.AppendChild(borderFactory);
                }
                count ++;
                FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(Button));
                buttonFactory.SetValue(Button.ContentProperty, action.DisplayName);
                buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(action.ExecuteAction_Click));
                buttonFactory.SetValue(Button.StyleProperty, this.Resources["LinkButton"]);
                spFactory.AppendChild(buttonFactory);
            }
            
            //set the visual tree of the data template
            cardLayout.VisualTree = spFactory;

            var actionColumn = AddColumn("ActionColumn");
            actionColumn.CellTemplate = cardLayout;
            
            ListViewSortManager = new ListViewSortManager(this.lstViewValues);
            ListViewSortManager.GetColumnDefaultSortDirection = GetColumnDefaultSortDirection;

            HideErrorMessage();
            HideHistoryBorder();

            this.OnCloseView += OnClosingView;
        }

        #region Helpers
        public void SetTitleBarText(string title)
        {
            this.TitleBar.Content = title;
        }

        private GridViewColumn AddColumn(string resourceName)
        {
            GridViewColumn gridViewColumn = GetColumn(resourceName);
            (lstViewValues.View as GridView).Columns.Add(gridViewColumn);
            return gridViewColumn;
        }

        private GridViewColumn GetColumn(string resourceName)
        {
            return this.Resources[resourceName] as GridViewColumn;
        }

        private void HideHistoryBorder()
        {
            Dispatcher.Invoke(() =>
            {
                OuterHistoryBorder.Visibility = Visibility.Hidden;
            });
        }

        private static ListSortDirection GetColumnDefaultSortDirection(string columnName)
        {
            switch (columnName)
            {
                case "LastModifiedDate":
                    {
                        return ListSortDirection.Descending;
                    }
            }
            return ListSortDirection.Ascending;
        }
        #endregion

        #region Basic Event Handlers
        private void FieldDefListView_Initialized(object sender, EventArgs e)
        {
            _hasLoaded = true;
            RefreshRuleAppListAsync();
        }

        private void RefreshRuleApplicationList_Click(object sender, RoutedEventArgs e)
        {
            RefreshRuleAppListAsync();
        }

        private void CheckApplyDefaultSorting()
        {
            if (!ListViewSortManager.HasSortApplied)
            {
                ListViewSortManager.SortByColumnNameAscending(DefaultSortColumnName);
            }
        }

        private void HideErrorMessage()
        {
            Dispatcher.Invoke(() =>
            {
                ErrorMessageBorder.Visibility = Visibility.Hidden;
            });
        }

        private void txtFilter_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
            }
        }

        private void TxtFilter_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(lstViewValues.ItemsSource);
            view.Refresh();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                OnCloseView?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Dispose()
        {
            //RuleAppDBService.OnDatabaseUpdated -= OnDatabaseUpdated;
        }

        private void OnClosingView(object sender, object e)
        {
        }
        #endregion

        #region Unused Field Loading Logic
        private void RefreshRuleAppListAsync()
        {
            if (!_hasLoaded) return;

            try
            {
                HideErrorMessage();
                waitSpinner.StartSpinning();
                this.InvokeOnWorkerThread(() => { LoadAllRuleApps(); });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                waitSpinner.StopSpinning();
            }
        }
        public void LoadAllRuleApps()
        {
            IList<UnusedFieldReference> unusedFieldList;
            string errorMsg;
            if (!TryGetUnusedFieldList(out unusedFieldList, out errorMsg))
            {
                Dispatcher.Invoke(() => { OnError(errorMsg); });
            }
            else
            {
                Dispatcher.Invoke(() => { OnUnusedFieldsRetrieved(unusedFieldList); });
            }
        }

        private bool TryGetUnusedFieldList(out IList<UnusedFieldReference> unusedFields, out string errorMsg)
        {
            unusedFields = this.TargetRuleApplicationDef
                               .GetUnusedFields()
                               .Select(t => new UnusedFieldReference(t))
                               .ToList();

            var usageNetwork = DefUsageNetwork.Create(this.TargetRuleApplicationDef);
            var allEntities = this.TargetRuleApplicationDef.GetChildDefsByType<EntityDef>();
            foreach (var entity in allEntities)
            {
                var thisUsages = usageNetwork.GetDefUsages(entity.Guid);
                if (!thisUsages.Any(usage => usage.UsageType == DefUsageType.ConsumedBy
                                            || usage.UsageType == DefUsageType.InvalidatedBy
                                            || usage.UsageType == DefUsageType.UpdatedBy
                                            || usage.UsageType == DefUsageType.TypeUsedBy
                                            || usage.UsageType == DefUsageType.ParentOf  //This forces all children to be removed before this can be considered unused.  Alternative would be to auto-select children
                                            )
                    && entity.Fields.Count == 0)
                {
                    unusedFields.Add(new UnusedFieldReference(entity));
                }
            }

            errorMsg = null;
            return true;
        }

        private void OnUnusedFieldsRetrieved(IList<UnusedFieldReference> ruleAppDescriptors)
        {
            try
            {
                this.lstViewValues.ItemsSource = new ObservableCollection<UnusedFieldReference>(ruleAppDescriptors);
                CollectionViewSource.GetDefaultView(lstViewValues.ItemsSource).Filter = TestRuleAppAgainstFilter;
                CheckApplyDefaultSorting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                waitSpinner.StopSpinning();
            }
        }

        private bool TestRuleAppAgainstFilter(object item)
        {
            var dbRuleApp = item as UnusedFieldReference;

            if (dbRuleApp == null) return false;

            var filter = this.txtFilter.Text;
            if (string.IsNullOrEmpty(filter)) return true;
            if (dbRuleApp.Name.ContainsIgnoreCase(filter)) return true;
            if (dbRuleApp.TypeName.ContainsIgnoreCase(filter)) return true;
            return false;
        }
        #endregion

        private void OnError(string errorMsg)
        {
            Dispatcher.Invoke(() =>
            {
                ErrorMessageBorder.Visibility = Visibility.Visible;
                var displayErrorMessage =
                    $"An error occured connecting to the rule application database.{Environment.NewLine}{Environment.NewLine}Please check your connection string settings in irAuthor under File->Options->UBF Options{Environment.NewLine}{Environment.NewLine}Error Message:{Environment.NewLine}{errorMsg}";
                ErrorMessageTextBlock.Text = displayErrorMessage;
                //MessageBox.Show(displayErrorMessage);
                waitSpinner.StopSpinning();
            });
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                waitSpinner.StartSpinning();
                var allItems = this.lstViewValues.ItemsSource.ToList<UnusedFieldReference>();
                _deleteAllAction(this, allItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            RefreshRuleAppListAsync();
            waitSpinner.StopSpinning();
        }

        public void RemoveItem(UnusedFieldReference target)
        {
            var source = lstViewValues.ItemsSource as ObservableCollection<UnusedFieldReference>;
            source.Remove(target);
        }
        public void RemoveRange(IEnumerable<UnusedFieldReference> target)
        {
            var source = lstViewValues.ItemsSource as ObservableCollection<UnusedFieldReference>;
            var sourceList = source.ToList();
            sourceList.RemoveAll(s => target.Contains(s));
            lstViewValues.ItemsSource = new ObservableCollection<UnusedFieldReference>(sourceList);
        }
    }

    public class UnusedFieldAction
    {
        public UnusedFieldAction(string displayName, Action<FieldDefListView, UnusedFieldReference> action)
        {
            this.DisplayName = displayName;
            this.Action = action;
        }
        public string DisplayName { get; set; }
        public Action<FieldDefListView, UnusedFieldReference> Action { get; set; }
        public FieldDefListView ListView { get; set; }

        public void ExecuteAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var unusedRef = button.DataContext as UnusedFieldReference;
                Action(ListView, unusedRef);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
    
}

