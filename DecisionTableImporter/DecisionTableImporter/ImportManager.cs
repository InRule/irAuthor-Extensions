using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;
using InRule.Repository;
using InRule.Repository.DecisionTables;

namespace DecisionTableImporter
{
    

    public class ImportManager
    {
        public RuleApplicationService RuleApplicationService { get; set; }
        public RuleRepositoryDefBase SelectedItem { get; set; }
        private RuleApplicationDef _ruleAppDef;
        private Dictionary<Guid, int> _conditionToQuestionDictionary = new Dictionary<Guid, int>();
        private string _excelFilePath;
        private string _sheetName;

        private const string _decisionTableNameBase = "Imported_";

        
        public void Execute()
        {
            try
            {
                _ruleAppDef = RuleApplicationService.RuleApplicationDef;
                _excelFilePath = Utility.BrowseForPath();
                _sheetName = Utility.GetSheetName();

                if (_excelFilePath.Length == 0)
                {
                    return;
                }

                var spreadsheet = new Spreadsheet();
                var rows = spreadsheet.ExtractExcelValues(_excelFilePath, _sheetName);

                var decisionTableDef = new DecisionTableDef();
                decisionTableDef.Name = GetDTName(_excelFilePath);
                RuleApplicationService.Controller.AddDef(decisionTableDef, SelectedItem);
                
            
                // create a wait window that will run on background thread
                var window = new BackgroundWorkerWaitWindow("Decision Table Import", "Importing...");

                // use delegate to load decision table with data
                window.DoWork += delegate(object sender, DoWorkEventArgs e)
                                     {
                                         ExecuteImport(decisionTableDef, rows, spreadsheet);
                                     };

                // use delegate to report errors and close window
                window.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
                {
                    if (e.Error != null)
                    {
                        MessageBoxFactory.Show("The following error occurred while attempting to import the spreadsheet:\n\n" + e.Error.ToString(),
                            "Import",
                            MessageBoxFactoryImage.Error);
                    }
                    window.Close();

                    var validations = _ruleAppDef.Validate();
                    var s = new StringBuilder();
                    foreach (var validation in validations)
                    {
                        s.AppendLine(validation.ToString());
                    }
                    if (s.Length > 0)
                    {
                        MessageBoxFactory.Show(s.ToString(), "");
                    }
                };

                // show the window
                window.ShowDialog();
            }
            catch (Exception e)
            {
                MessageBoxFactory.Show("The following error occurred while attempting to import the spreadsheet:\n\n" + e,
                    "Regimen Import",
                    MessageBoxFactoryImage.Error);
            }
        }


        private DecisionTableDef AddNewDecisiontableToRuleApp(int dtId)
        {
            var decisionTableDef = new DecisionTableDef();
            decisionTableDef.Name = GetDTName(_excelFilePath) + dtId;
            RuleApplicationService.Controller.AddDef(decisionTableDef, SelectedItem);

            return decisionTableDef;
        }

        #region Main Logic Flow

        private void ExecuteImport(DecisionTableDef decisionTableDef, List<Row> rows, Spreadsheet spreadsheet)
        {
            // gather details from spreadsheet condition columns and create decision table conditions
            foreach (var condition in spreadsheet.Conditions)
            {
                AddCondition(condition.Key, decisionTableDef, spreadsheet);
            }
         
            // gather details from spreadsheet action columns and create decision table actions
            foreach (var action in spreadsheet.Actions)
            {
                AddAction(action.Key, decisionTableDef, spreadsheet);
            }
            
            //create the rows in the decision table based on the spreadsheet rows
            foreach (var row in rows)
            {
                AddDecision(row, decisionTableDef);
            }
        }

        #endregion

        #region Condition methods

        
        private void AddAction(int actionColumn, DecisionTableDef decisionTableDef, Spreadsheet spreadsheet)
        {
            var action = spreadsheet.Actions[actionColumn];

            var actionDimensionDef = new ActionDimensionDef();
            actionDimensionDef.UseLanguageRules = false;
            actionDimensionDef.UseFieldsValueList = false;

            actionDimensionDef.FieldName = spreadsheet.GetActionName(actionColumn);
            actionDimensionDef.DisplayName = actionDimensionDef.FieldName;
            decisionTableDef.Actions.Add(actionDimensionDef);

            foreach (var value in action.Values)
            {
                var actionValueDef = new ActionValueDef();
                actionValueDef.Tokens.Add("DisplayValue", value);
                actionValueDef.Tokens.Add("Value", value);
                actionValueDef.IsFromMasterList = true;

                actionDimensionDef.ActionValues.Add(actionValueDef);
            }
        }

        private void AddCondition(int conditionColumn, DecisionTableDef decisionTableDef, Spreadsheet spreadsheet)
        {
            var condition = spreadsheet.Conditions[conditionColumn];

            var conditionDimensionDef = new ConditionDimensionDef();
            conditionDimensionDef.AllowNullValues = true;
            conditionDimensionDef.TargetDataType = DataType.String;
            conditionDimensionDef.Name = spreadsheet.GetConditionName(conditionColumn);
            decisionTableDef.Conditions.Add(conditionDimensionDef);

            foreach (var value in condition.Values)
            {
                var yesValueDef = new ConditionValueDef();
                yesValueDef.ConditionType = ConditionTypes.EqualTo;
                yesValueDef.Tokens.Add("DisplayValue", value);
                yesValueDef.Tokens.Add("Value", value);

                conditionDimensionDef.ConditionValues.Add(yesValueDef);
            }

        }
  

  
        #endregion


        #region Decision methods

        private void AddDecision(Row row, DecisionTableDef decisionTableDef)
        {
            var decisionDef = new DecisionDef();  //Decision Table row
            try
            {
                //Go through the condition columns one at a time
                int cellNumber = 0;

                foreach (var conditionDimension in decisionTableDef.Conditions)
                {
                    var conditionNodeDef = new ConditionNodeDef();  //column in Decision table row
                    
                    try
                    {
                        conditionNodeDef.SourceValueID = GetConditionValueGuidForSpreadsheetCell(row.Conditions[cellNumber].Value, conditionDimension);
                        conditionNodeDef.SourceDimensionID = conditionDimension.DimensionID;
                    }
                    catch 
                    {
                        // Caused by empty cell, so do nothing.  Will show as -Any-
                    }
                    decisionDef.ConditionNodes.Add(conditionNodeDef);

                    cellNumber++;
                }

                cellNumber = 0;
                foreach (ActionDimensionDef actionDimension in decisionTableDef.Actions)
                {
                    var actionNodeDef = new ActionNodeDef();
                  
                    try
                    {
                        actionNodeDef.SourceValueID = GetActionValueGuidForSpreadsheetCell(row.Actions[cellNumber].Value, actionDimension);
                        actionNodeDef.SourceDimensionID = actionDimension.DimensionID;
                    }
                    catch 
                    {
                        // Caused by empty cell, so do nothing  Will show as -Ignore-
                    }
                    decisionDef.ActionNodes.Add(actionNodeDef);
                    cellNumber++;
                }

                decisionTableDef.Decisions.Add(decisionDef);
          
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        #endregion

        #region Helper methods

      
        private Guid GetConditionValueGuidForSpreadsheetCell(string cellContents, ConditionDimensionDef condDimDef)
        {
            Guid retVal = new Guid();
            foreach (var cellValue in condDimDef.ConditionValues)
            {
                if (cellValue.Value.ToString().Equals(cellContents))
                {
                    retVal = cellValue.ValueID;
                    break;
                }
            }
            return retVal;
        }

        private Guid GetActionValueGuidForSpreadsheetCell(string cellContents, ActionDimensionDef actionDimDef)
        {
            Guid retVal = new Guid();
            try
            {
                foreach (var cellValue in actionDimDef.ActionValues)
                {
                    if (cellValue.Value.ToString().Equals(cellContents))
                    {
                        retVal = cellValue.ValueID;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return retVal;
        }
      

        private string GetDTName(string excelFilePath)
        {
            
            var fileNameStart = excelFilePath.LastIndexOf("\\") + 1;
            var fileNameEnd = excelFilePath.IndexOf(".xls");

            var fileName = excelFilePath.Substring(fileNameStart, fileNameEnd - fileNameStart).Replace(" ", "_");
            fileName = Utility.RemoveSpecialCharacters(fileName);

           // return String.Format(_decisionTableNameBase, fileName);
            return _decisionTableNameBase +  fileName;
        }
        #endregion

    }


}
