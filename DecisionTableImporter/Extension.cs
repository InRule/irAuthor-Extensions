using System;
using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Controls;
using InRule.Repository;
using InRule.Repository.RuleElements;

namespace DecisionTableImporter
{
    class Extension : ExtensionBase
    {
        private const string ExtensionGUID = "{C5DA0A76-F0AB-47B7-A6CB-D6E156D4A82B}";
        private VisualDelegateCommand _importCommand;
        private IRibbonGroup _group;
        private IRibbonButton _button;
       

        // To make system extension that cannot be disabled, change last parm to true
        public Extension()
            : base("Decision Table Importer", "Imports decision table from excel", new Guid(ExtensionGUID), false)
        {
        }

        public override void Enable()
        {
            _group = IrAuthorShell.HomeTab.AddGroup("Import", null, "");

            _importCommand = new VisualDelegateCommand(Import, "Import", ImageFactory.GetImageThisAssembly(@"/import.png"), ImageFactory.GetImageThisAssembly(@"/import.png"), false);
            _button = _group.AddButton(_importCommand, "Import Decision Table");
            
            RuleApplicationService.Opened += EnableButton;
            RuleApplicationService.Closed += EnableButton;
            SelectionManager.SelectedItemChanged += EnableButton;
        }

        private void EnableButton(object sender, EventArgs e)
        {
            if (RuleApplicationService.RuleApplicationDef != null && 
                (SelectionManager.SelectedItem is SimpleRuleDef ||
                SelectionManager.SelectedItem is RuleSetDef||
                SelectionManager.SelectedItem is ExclusiveRuleDefaultRootDef))
            {
                
                _importCommand.IsEnabled = true;
            }
            else
            {
                _importCommand.IsEnabled = false;
            }
        }

        private void Import(object o)
        {
            var importManager = ServiceManager.Compose<ImportManager>();
            importManager.SelectedItem = SelectionManager.SelectedItem as RuleRepositoryDefBase;
            importManager.Execute();
        }        
        
        public override void Disable()
        {
            IrAuthorShell.HomeTab.RemoveGroup(_group);
        }
    }
}
