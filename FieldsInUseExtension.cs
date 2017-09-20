using System;
using System.Windows;
using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Controls;
using InRule.Common.Utilities;
using InRule.Repository;
using InRuleLabs.AuthoringExtensions.FieldsInUse.Views;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse
{
    class FieldsInUseExtension : ExtensionBase
    {
        private const string ExtensionGUID = "{082c4be6-2f2d-45d6-beb8-2761dccb0e40}";
        
        private IRibbonGroup _analyzeGroup;
        private VisualDelegateCommand _showFieldUsageCommand;
        
        // To make system extension that cannot be disabled, change last parm to true
        public FieldsInUseExtension()
            : base("Fields In Use", "Show which fields are used by rules", new Guid(ExtensionGUID), false)
        {
         
        }
        
        public override void Enable()
        {
            try
            {
                RuleApplicationService.Opened += WhenRuleAppLoaded;
                RuleApplicationService.Closed += WhenRuleAppClosed;
                AddHomeTabButtons();
                CheckEnableCommands();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
      

        internal void ShowFieldUsage(object obj)
        {
            try
            {
                var ruleAppDef = this.RuleApplicationService.RuleApplicationDef;
                var window = new FieldUsageSummary();
                window.Populate(ruleAppDef);
                window.Show();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
        
        private void AddHomeTabButtons()
        {
            var ribbonTab = IrAuthorShell.HomeTab;
            if (ribbonTab != null)
            {
                _analyzeGroup = ribbonTab.AddGroup("App Analysis", null, "");

                _showFieldUsageCommand = new VisualDelegateCommand(this.ShowFieldUsage, "Field Usage",
                    ImageFactory.GetImageAuthoringAssembly(@"/Images/Trace16.png"),
                    ImageFactory.GetImageAuthoringAssembly(@"/Images/Trace32.png"), true);
                _analyzeGroup.AddButton(_showFieldUsageCommand);
            }
        }

        
        public override void Disable()
        {
            IrAuthorShell.HomeTab.RemoveGroup(_analyzeGroup);
        }
        
        private void WhenRuleAppClosed(object sender, EventArgs<RuleApplicationDef> e)
        {
            CheckEnableCommands();
        }
        private void WhenRuleAppLoaded(object sender, EventArgs e)
        {
            CheckEnableCommands();
        }
        private void CheckEnableCommands()
        {
            if (_showFieldUsageCommand != null)
            {
                _showFieldUsageCommand.IsEnabled = RuleApplicationService.RuleApplicationDef != null;
            }
        }

       

       
        
    }
}
