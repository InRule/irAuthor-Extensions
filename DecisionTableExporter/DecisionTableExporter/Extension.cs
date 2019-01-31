using System;
using System.Collections.Generic;
using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Controls;
using InRule.Repository;
using InRule.Repository.DecisionTables;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace Inrule.Authoring.Extensions
{
    class DecisionTableExporter : ExtensionBase
    {
        private const string ExtensionGUID = "{24B91A24-80A2-43D6-AAEA-9E789EF1962D}";
        private VisualDelegateCommand exportCommand;
        private IRibbonContextualTabGroup decisionTableTabGroup;
        private IRibbonTab settingsTab;
        private IRibbonGroup toolsGroup;

        //
        // To make system extension that is "on" immediately at deployment, change last parm to true
        //
        public DecisionTableExporter()
            : base("DecisionTableExporter", "DecisionTableExporter", new Guid(ExtensionGUID), false)
        {

        }
        
        //
        // The Decision table contextual menu is created on the fly when the content is a decision table. It is not available otherwise, and has not yet
        // been created at the SelectionManager.SelectedItemChanged event, so we wait until the content control changes, then check to see if what is selected is a 
        // decision table.
        //
        public override void Enable()
        {
            IrAuthorShell.ContentControlChanged += WhenContentControlChanged;
        }

        private void WhenContentControlChanged(object sender, EventArgs e)
        {
            if (SelectionManager.SelectedItem is DecisionTableDef)
            {
                decisionTableTabGroup = IrAuthorShell.Ribbon.GetContextualTabGroup("Decision Table");

                if (decisionTableTabGroup != null)
                {
                    settingsTab = decisionTableTabGroup.GetTab("Settings");
                    toolsGroup = settingsTab.GetGroup("Tools");
                    exportCommand = new VisualDelegateCommand(ExportTable, "Export to csv", ImageFactory.GetImageThisAssembly(@"Images/csv32.png"), ImageFactory.GetImageThisAssembly(@"Images/csv48.png"), false);
                    exportCommand.IsEnabled = true;
                    toolsGroup.AddButton(exportCommand);
                }
            }
        }

        // main logic for exporting to csv
        //
        private void ExportTable(object obj)
        {
            StringBuilder spreadsheetText = new StringBuilder();

            var selectedItem = SelectionManager.SelectedItem as DecisionTableDef;
            if (selectedItem != null && selectedItem is DecisionTableDef)
            {

                // Get a set order for displaying the conditions and actions by getting an ordered list of guids for each column definition
                var condIds = getConditionGuids(selectedItem, spreadsheetText);
                var actionIds = getActionGuids(selectedItem, spreadsheetText);

                // get information from each row  (DecisionDef)
                // we ensure the nodes are presented in the same order for each row by retrieving each node by its DimensionId from the two List objects above.
                foreach (DecisionDef def in selectedItem.Decisions)
                {
                    addConditionCells(spreadsheetText, condIds, def);
                    addActionCells(spreadsheetText, actionIds, def);
                    spreadsheetText.Append("\r\n");
                }
   
                //saveToFile(selectedItem.Name, spreadsheetText);  // decision table name only (may not be unique)

                saveToFile(selectedItem.GetFullName(), spreadsheetText);  // uses full path to decision table as name
                
            }
        }

        //
        // create the condition cells on the spreadsheet for a given row (DecisionDef)
        //
        private void addConditionCells(StringBuilder spreadsheetText, List<System.Guid> condIds, DecisionDef def)
        {
            foreach (System.Guid condId in condIds)
            {
                try
                {
                    spreadsheetText.Append("\"" + def.ConditionNodes.GetByDimensionId(condId).GetValueDisplayName() + "\",");
                }
                catch (Exception ex)
                {
                    spreadsheetText.Append("-Any-,");
                    var info = ex.StackTrace;
                }
            }

            spreadsheetText.Append(",");   //spacer cell before actions
        }

        //
        // create the action cells on the spreadsheet for a given row (DecisionDef)
        //
        private void addActionCells(StringBuilder spreadsheetText, List<System.Guid> actionIds, DecisionDef def)
        {
            foreach (System.Guid actId in actionIds)
            {
                try
                {
                    spreadsheetText.Append("\"" + def.ActionNodes.GetByDimensionId(actId).GetValueDisplayName() + "\",");
                }
                catch (Exception ex2)
                {
                    spreadsheetText.Append("-Ignore-,");
                    var info = ex2.StackTrace;
                }
            }            
        }

        //
        // save the contents to a .csv file
        //
        private void saveToFile(string defaultName, StringBuilder content)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
           
            saveFileDialog.FileName = defaultName + ".csv";  
            saveFileDialog.Filter = "Comma-separated values file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, content.ToString());
            }
        }

        //
        // creates initial headers for condition columns on spreadsheet
        // returns ordered list of Guids for retrieving condition columns. 
        //
        private List<System.Guid> getConditionGuids(DecisionTableDef selectedItem, StringBuilder spreadsheetText)
        {
            List<System.Guid> condGuids = new List<System.Guid>();
            foreach (ConditionDimensionDef condDef in selectedItem.Conditions)
            {
                condGuids.Add(condDef.DimensionID);
                spreadsheetText.Append("\"" + condDef.Name.ToUpper() + "\","); //column title
            }
            spreadsheetText.Append(",");  // just a spacer cell 

            return condGuids;
        }

        //
        // creates initial headers for action columns on spreadsheet 
        // returns ordered list of Guids for retrieving action columns. 
        //
        private List<System.Guid> getActionGuids(DecisionTableDef selectedItem, StringBuilder spreadsheetText)
        {
            List<System.Guid> actionGuids = new List<System.Guid>();
            foreach (ActionDimensionDef adDef in selectedItem.Actions)
            {
                actionGuids.Add(adDef.DimensionID);
                spreadsheetText.Append("\"" + adDef.DisplayName.ToUpper() + "\",");  //column title
            }
            spreadsheetText.Append("\r\n\r\n");
            return actionGuids;
        }
    }
}
