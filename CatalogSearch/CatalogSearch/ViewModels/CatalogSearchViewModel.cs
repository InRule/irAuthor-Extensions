using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using CatalogSearch.Views;
using InRule.Repository;
using InRule.Repository.Client;
using InRule.Runtime;
using NuGet;
using System.ComponentModel;
using System.Diagnostics;
using CatalogSearch.Commands;
using InRule.Repository.RuleElements;
using InRule.Authoring.Services;

namespace CatalogSearch.ViewModels
{
    public class CatalogSearchViewModel : INotifyPropertyChanged, IDisposable
    {
        internal readonly CatalogSearchSettings Settings;
        public Action<object> PerformNavigate;

        public event PropertyChangedEventHandler PropertyChanged;

        public CatalogSearchWindow CatalogSearchView { private get; set; }
        public ObservableCollection<CatalogSearchResultViewModel> Results { get; }

        public ICommand NavigateCommand { get; }

        private int _operationProgress = 0;
        public int Progress {
            get { return _operationProgress; }
            set {
                if (_operationProgress == value) return;
                _operationProgress = value;
                Debug.WriteLine("Progress Changing");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
            }
        }
        public bool ShowProgress => Progress > 0;


        private SearchField _selectedSearchField = SearchField.Name;
        public SearchField SelectedSearchField
        {
            get { return _selectedSearchField; }
            set
            {
                _selectedSearchField = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedSearchField"));
            }
        }
        public IEnumerable<SearchField> SearchFieldValues
        {
            get
            {
                return Enum.GetValues(typeof(SearchField)).Cast<SearchField>();
            }
        }

        private string _queryString = "";
        public string QueryString
        {
            get { return _queryString; }
            set
            {
                _queryString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("QueryString"));
            }
        }

        private bool _isSearchEnabled = true;
        public bool IsSearchEnabled
        {
            get { return _isSearchEnabled; }
            set
            {
                _isSearchEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSearchEnabled"));
            }
        }

        public CatalogSearchViewModel(CatalogSearchSettings settings, Action<object> performNavigate)
        {
            Settings = settings;
            PerformNavigate = performNavigate;
            Results = new ObservableCollection<CatalogSearchResultViewModel>();

            NavigateCommand = new NavigateCommand(this);

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != "Progress") return;
                Debug.WriteLine("ShowProgress changing");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowProgress"));
            };
        }

        public void WorkStarted()
        {
            IsSearchEnabled = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSearchEnabled"));
        }

        public void WorkComplete()
        {
            IsSearchEnabled = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSearchEnabled"));
        }

        public void Search(bool showInstalledOnly = false)
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            WorkStarted();
            
            Task.Factory.StartNew(() =>
            {
                Debug.WriteLine("Starting search...");
                dispatcher.BeginInvoke(new Action(() => { Results.Clear(); Progress = 0; }));

                SearchCatalogForDescription(SelectedSearchField, QueryString, dispatcher);

                WorkComplete();
                Debug.WriteLine("Search complete.");

            });
        }

        public void Dispose()
        {
        }
        
        public List<CatalogSearchResultViewModel> SearchCatalogForDescription(SearchField field, string searchQuery, Dispatcher dispatcher)
        {
            List<CatalogSearchResultViewModel> results = new List<CatalogSearchResultViewModel>();

            try
            {
                Debug.WriteLine($"Searching for '{searchQuery}' in the {field} from catalog located at {Settings.CatalogServiceUrl}");

                using (var catCon = new RuleCatalogConnection(new Uri(Settings.CatalogServiceUrl), TimeSpan.FromSeconds(60), Settings.CatalogUsername, Settings.CatalogPassword))
                {
                    var ruleApps = catCon.GetAllRuleApps().Where(ra => ra.Key.IsLatest);
                    int processedRuleApps = 0;

                    foreach (var ruleApp in ruleApps)
                    {
                        var ruleAppDefInfo = ruleApp.Key;
                        var ruleAppInfo = ruleApp.Value;

                        Debug.WriteLine($"Searching Rule App {ruleAppDefInfo.Name} v{ruleAppDefInfo.PublicRevision} {ruleAppInfo.LastLabelName}");

                        var ruleAppRef = new CatalogRuleApplicationReference(Settings.CatalogServiceUrl, ruleAppDefInfo.Name, Settings.CatalogUsername, Settings.CatalogPassword, ruleAppDefInfo.PublicRevision);
                        var ruleAppDef = ruleAppRef.GetRuleApplicationDef();
                        var entities = ((IEnumerable<EntityDef>)ruleAppDef.Entities);
                        foreach (var entity in entities)
                        {
                            foreach (var ruleSet in entity.GetAllRuleSets())
                            {
                                switch (field)
                                {
                                    case SearchField.Description:
                                        if (ruleSet.Comments.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                                            dispatcher.BeginInvoke(new Action(() => { Results.Add(new CatalogSearchResultViewModel(ruleAppDefInfo, ruleSet, ruleSet.Comments, Settings)); }));
                                        break;
                                    case SearchField.Name:
                                        if (ruleSet.Name.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                                            dispatcher.BeginInvoke(new Action(() => { Results.Add(new CatalogSearchResultViewModel(ruleAppDefInfo, ruleSet, ruleSet.Name, Settings)); }));
                                        break;
                                    case SearchField.Note:
                                        var matchingNote = ruleSet.Notes.FirstOrDefault(n => n.Text.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase));
                                        if (matchingNote != null)
                                            dispatcher.BeginInvoke(new Action(() => { Results.Add(new CatalogSearchResultViewModel(ruleAppDefInfo, ruleSet, matchingNote.Text, Settings)); }));
                                        break;
                                }

                                foreach (RuleElementDef rule in ruleSet.GetAllRuleElements())
                                {
                                    switch (field)
                                    {
                                        case SearchField.Description:
                                            if (rule.Comments.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                                                dispatcher.BeginInvoke(new Action(() => { Results.Add(new CatalogSearchResultViewModel(ruleAppDefInfo, ruleSet, rule.Comments, Settings, rule)); }));
                                            break;
                                        case SearchField.Name:
                                            if (rule.Name.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                                                dispatcher.BeginInvoke(new Action(() => { Results.Add(new CatalogSearchResultViewModel(ruleAppDefInfo, ruleSet, rule.Name, Settings, rule)); }));
                                            break;
                                        case SearchField.Note:
                                            var matchingNote = rule.Notes.FirstOrDefault(n => n.Text.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase));
                                            if (matchingNote != null)
                                                dispatcher.BeginInvoke(new Action(() => { Results.Add(new CatalogSearchResultViewModel(ruleAppDefInfo, ruleSet, matchingNote.Text, Settings, rule)); }));
                                            break;
                                    }
                                }
                            }
                        }

                        dispatcher.BeginInvoke(new Action(() => { Progress = (int)(++processedRuleApps / (double)ruleApps.Count() * 100); }));
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error occurred: " + ex.ToString());
            }

            return results;
        }
    }
}