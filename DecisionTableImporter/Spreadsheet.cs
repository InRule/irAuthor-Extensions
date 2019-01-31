using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DecisionTableImporter
{
    public class Child
    {
        public string Name { get; set; }
        public List<string> Values { get; internal set; }

        public Child()
        {
            Values = new List<string>();
        }
    }

    class Spreadsheet
    {
        private const string _conditionText = "Condition:";
        private const string _actionText = "Action:";
        public Dictionary<int, Child> Conditions { get; set; }
        public Dictionary<int, Child> Actions { get; set; }
        public List<Row> Rows { get; set; }

        public Spreadsheet()
        {
            Conditions = new Dictionary<int, Child>();
            Actions = new Dictionary<int, Child>();
        }

        public List<Row> ExtractExcelValues(string xlsxFilePath, string sheetName)
        {
            Rows = new List<Row>();

            using (SpreadsheetDocument myWorkbook = SpreadsheetDocument.Open(xlsxFilePath, true))
            {
                //Access the main Workbook part, which contains data
                WorkbookPart workbookPart = myWorkbook.WorkbookPart;
                WorksheetPart worksheetPart = null;

                var sheets = workbookPart.Workbook.Descendants<Sheet>();

                foreach (var sheet in sheets)
                {
                    if (sheet.Name.ToString().ToLower() == sheetName.ToLower())
                    {
                        worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                    }
                }

                var columnCount = 0;

                var stringTablePart = workbookPart.SharedStringTablePart;

                if (worksheetPart != null)
                {

                    DocumentFormat.OpenXml.Spreadsheet.Row lastRow = worksheetPart.Worksheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>().LastOrDefault();
                    DocumentFormat.OpenXml.Spreadsheet.Row firstRow = worksheetPart.Worksheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>().FirstOrDefault();

                    if (firstRow != null)
                    {
                        foreach (Cell c in firstRow.ChildElements)
                        {
                            columnCount++;

                            var value = GetValue(c, stringTablePart);

                            if (value.StartsWith(_conditionText))
                            {
                                Conditions.Add(columnCount, new Child()
                                                            {
                                                                Name = value.Replace(_conditionText, "")
                                                            });
                            }

                            if (value.StartsWith(_actionText))
                            {
                                Actions.Add(columnCount, new Child()
                                {
                                    Name = value.Replace(_actionText, "")
                                });
                            }
                        }
                    }

                    if (lastRow != null)
                    {
                        for (int i = 2; i <= lastRow.RowIndex; i++)
                        {
                            var row = new Row();

                            bool empty = true;

                            DocumentFormat.OpenXml.Spreadsheet.Row spreadSheetRow = worksheetPart.Worksheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>().Where(r => i == r.RowIndex).FirstOrDefault();

                            int j = 0;

                            if (spreadSheetRow != null)
                            {
                                var columnNumber = 0;

                                foreach (Cell c in spreadSheetRow.ChildElements)
                                {
                                    columnNumber++;


                                    //Get cell value
                                    string value = GetValue(c, stringTablePart);
                                    if (IsCondition(columnNumber))
                                    {
                                        row.Conditions.Add(new Condition
                                                           {
                                                               ColumnNumber = columnNumber,
                                                               Value = value
                                                           });

                                        AddConditionValue(columnNumber, value);
                                    }
                                    else
                                    {
                                        row.Actions.Add(new Action
                                        {
                                            ColumnNumber = columnNumber,
                                            Value = value
                                        });

                                        AddActionValue(columnNumber, value);
                                    }

                                    if (!string.IsNullOrEmpty(value) && value != "")
                                    {
                                        empty = false;
                                    }
                                    j++;
                                    if (j == columnCount)
                                    {
                                        break;
                                    }
                                }

                                if (empty)
                                {
                                    break;
                                }
                                Rows.Add(row);
                            }
                        }
                    }
                }
            }
            return Rows;
        }

        private void AddConditionValue(int columnNumber, string value)
        {
            var condition = Conditions[columnNumber];

            if (!condition.Values.Contains(value))
            {
                condition.Values.Add(value);
            }
        }

        private void AddActionValue(int columnNumber, string value)
        {
            var action = Actions[columnNumber];

            if (!action.Values.Contains(value))
            {
                action.Values.Add(value);
            }
        }

        private bool IsCondition(int columnNumber)
        {
            if (columnNumber <= Conditions.Count)
            {
                return true;
            }
            return false;
        }


        public static string GetValue(Cell cell, SharedStringTablePart stringTablePart)
        {
            if (cell.ChildElements.Count == 0) return null;

            //get cell value
            string value = cell.ElementAt(0).InnerText;//CellValue.InnerText;

            //Look up real value from shared string table
            if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                value = stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;

            }
            return value;
        }

        public string GetConditionName(int conditionColumn)
        {
            foreach (var condition in Conditions)
            {
                if (condition.Key == conditionColumn)  
                {
                    return condition.Value.Name;
                }
            }
            return "";
        }
        public string GetActionName(int actionColumn)
        {
            foreach (var action in Actions)
            {
                if (action.Key == actionColumn)
                {
                    return action.Value.Name;
                }
            }
            return "";
        }
    }
}
