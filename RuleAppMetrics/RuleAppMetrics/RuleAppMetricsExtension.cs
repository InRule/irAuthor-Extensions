using System;
using System.Windows;
using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Controls;
using InRule.Common.Utilities;
using InRule.Repository;
using InRuleLabs.AuthoringExtensions.RuleAppMetrics.Views;
using InRuleLabs.AuthoringExtensions.RuleAppMetrics.Extensions;

namespace InRuleLabs.AuthoringExtensions.RuleAppMetrics
{
    class RuleAppMetricsExtension : ExtensionBase
    {
        private const string ExtensionGUID = "{082c4be6-2f2d-45d6-beb8-2722dccb0e40}";
        
        private IRibbonGroup _analyzeGroup;
        private VisualDelegateCommand _showMetricsCommand;

        // To make system extension that cannot be disabled, change last parm to true
        public RuleAppMetricsExtension()
            : base("Rule App Metrics", "Show Rule Application Metrics", new Guid(ExtensionGUID), false)
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
                var window = new TextPopupWindow();
                // TODO: if we can get initial entity state and execution information, we could also get CalculateRuleAppExecutionMetrics
                window.Populate("Rule App Metrics", ruleAppDef.CalculateRuleAppComplexity(RuleApplicationService.PersistenceInfo).ToString());
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
                if (ribbonTab.ContainsGroup("App Analysis"))
                    _analyzeGroup = ribbonTab.GetGroup("App Analysis");
                else
                    _analyzeGroup = ribbonTab.AddGroup("App Analysis", null, "");

                _showMetricsCommand = new VisualDelegateCommand(this.ShowFieldUsage, "Metrics",
                    ImageFactory.GetImageAuthoringAssembly(@"/Images/Trace16.png"),
                    ImageFactory.GetImageAuthoringAssembly(@"/Images/Trace32.png"), true);
                _analyzeGroup.AddButton(_showMetricsCommand);
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
            SetEnabledIfRuleAppIsLoaded(_showMetricsCommand);
        }

        private void SetEnabledIfRuleAppIsLoaded(VisualDelegateCommand command)
        {
            if (command != null)
            {
                command.IsEnabled = RuleApplicationService.RuleApplicationDef != null;
            }
        }
    }
}
