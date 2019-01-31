using System;
using System.Windows;
using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using CatalogSearch.ViewModels;
using CatalogSearch.Views;
using System.Collections;
using InRule.Repository;
using InRule.Repository.RuleElements;
using System.Linq;
using System.Diagnostics;

namespace CatalogSearch
{
    public sealed class CatalogSearchExtension : ExtensionBase
    {
        public VisualDelegateCommand OpenSearchCommand { get; private set; }
        
        public CatalogSearchExtension() 
            : base(name: "Catalog Search for IrAuthor", 
                  description: "Search through all Rule Applications stored in the Catalog for specific text in a Name, Description, or Note.", 
                  guid: new Guid("{8ADED372-E24E-427D-BAB8-D4277AD0F423}"))
        {
            Priority = 100000000; // HACK: ensures that the extension loads after other system extensions so that it can be the furthest element to the right.
        }

        public override void Enable()
        {
            //Icon from https://freeicon.org/icon/15015/magnifying-glass
            OpenSearchCommand = new VisualDelegateCommand(OpenSearch, "Search Catalog", 
                ImageFactory.GetImageThisAssembly("images/magnifyingglass.png"), 
                ImageFactory.GetImageThisAssembly("images/magnifyingglass.png"),
                RuleApplicationService.Connection != null);

            RuleApplicationService.HasConnectionChanged += RuleApplicationService_HasConnectionChanged;

            var catalogTab = IrAuthorShell.Ribbon.GetTab("Catalog");
            var group = catalogTab.GetGroup("General");
            var button = group.AddButton(OpenSearchCommand);
        }

        private void RuleApplicationService_HasConnectionChanged(object sender, EventArgs e)
        {
            OpenSearchCommand.IsEnabled = RuleApplicationService.Connection != null;
        }

        private void OpenSearch(object obj)
        {
            Guid? currentRuleApp = null;

            try
            {
                currentRuleApp = ((RuleRepositoryDefBase)SelectionManager.SelectedItem).ThisRuleApp.Guid;
            }
            catch { }

            var viewModel = new CatalogSearchViewModel(new CatalogSearchSettings(RuleApplicationService.Connection, currentRuleApp), PerformNavigate);
            
            var window = new CatalogSearchWindow(viewModel)
            {
                Owner = Application.Current.MainWindow
            };
            window.Show();
        }

        private void PerformNavigate(object target)
        {
            object localTarget = null;

            if (target.GetType() == typeof(RuleElementDef))
            {
                localTarget = RuleApplicationService.RuleApplicationDef.GetRuleApp().GetAllRuleElements().FirstOrDefault(r => r.Guid.Equals(((RuleElementDef)target).Guid));
            }
            else if (target.GetType() == typeof(RuleSetDef))
            {
                localTarget = RuleApplicationService.RuleApplicationDef.GetRuleApp().GetAllRuleSets().FirstOrDefault(r => r.Guid.Equals(((RuleSetDef)target).Guid));
            }
            else
            {
                Debug.WriteLine("Couldn't find item to navigate to.  This will look like it's working, but it won't affect the actual rule app.");
                localTarget = target;
            }

            if (localTarget != null)
                SelectionManager.SelectedItem = localTarget;
        }
    }
}