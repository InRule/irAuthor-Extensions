using InRule.Repository;
using InRule.Repository.RuleElements;
using InRule.Repository.Service.Data;
using System.ComponentModel;

namespace CatalogSearch.ViewModels
{
    public class CatalogSearchResultViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string RuleAppName { get; set; }
        public string RuleSetName { get; set; }
        public string RuleElementName { get; set; }
        public string MatchedValue { get; set; }

        public RuleSetDef RuleSetDef { get; set; }
        public RuleElementDef RuleDef { get; set; }

        public bool ShowRuleElementName
        {
            get
            {
                return !string.IsNullOrEmpty(RuleElementName);
            }
        }

        private bool isInCurrentRuleApp;
        public bool IsInCurrentRuleApp
        {
            get { return isInCurrentRuleApp; }
            set
            {
                if (isInCurrentRuleApp == value) return;

                isInCurrentRuleApp = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsInCurrentRuleApp"));
            }
        }

        public CatalogSearchResultViewModel()
        {

        }
        public CatalogSearchResultViewModel(DefInfo ruleAppDef, RuleSetDef ruleSetDef, string matchedValue, CatalogSearchSettings settings, RuleElementDef ruleDef = null)
        {
            RuleAppName = ruleAppDef.Name + " v" + ruleAppDef.PublicRevision;
            RuleSetName = ruleSetDef.AuthoringElementPath;
            MatchedValue = matchedValue;
            //TODO: The way we're navigating to the item does not work properly - add this back in once that works
            IsInCurrentRuleApp = false;// settings.CurrentRuleApp != null && ruleAppDef.Key.Guid == settings.CurrentRuleApp.Value;

            RuleSetDef = ruleSetDef;

            if (ruleDef != null)
            {
                RuleElementName = ruleDef.AuthoringElementPath;
                RuleDef = ruleDef;
            }
        }
    }
}
