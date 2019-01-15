﻿using System;
using System.Data;
using System.Linq;
using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Controls;
using InRule.Repository;
using Microsoft.Win32;

namespace InRule.Authoring.Extensions.ExportTable
{
    public class Extension : ExtensionBase
    {
        private VisualDelegateCommand _command;
        private IRibbonButton _button;
        private IRibbonGroup _group;
        private bool _groupExisted;

        public Extension()
            : base("ExportTable", "Exports an inline table", new Guid("{C0FA0EB8-6280-4E23-BBD2-32D051D2BD0A}"))
        { }

        public override void Enable()
        {
            _group = IrAuthorShell.HomeTab.GetGroup("General");

            if (_group == null)
            {
                _group = IrAuthorShell.HomeTab.AddGroup("General", null);
            }
            else
            {
                _groupExisted = true;
            }

            _command = new VisualDelegateCommand(Execute, "Export Table", ImageFactory.GetImageThisAssembly("Images/Image16.png"), ImageFactory.GetImageThisAssembly("Images/Image32.png"));
            _command.IsEnabled = false;

            _button = _group.AddButton(_command);

            SelectionManager.SelectedItemChanged += WhenSelectedItemChanged;
        }

        private void WhenSelectedItemChanged(object sender, EventArgs e)
        {
            _command.IsEnabled = (SelectionManager.SelectedItem is TableDef);
        }

        public override void Disable()
        {
            if (_groupExisted)
            {
                IrAuthorShell.HomeTab.RemoveGroup(_group);
            }
            else
            {
                _group.RemoveItem(_button);
            }
        }

        private void Execute(object sender)
        {
            var tableDef = (TableDef) SelectionManager.SelectedItem;
            var dataSet = tableDef.TableSettings.InlineDataSet;
            
            if (dataSet.Tables.Count == 0) 
            {
                MessageBoxFactory.Show("Unable to export inline table because it does not contain a data set", "Export");
                return;
            }

            var rowCount = dataSet.Tables[0].Rows.Count;
            if (rowCount == 0)
            {
                MessageBoxFactory.Show("Unable to export inline table because it does not contain any rows", "Export");
                return;
            }

            var saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Specify file to export data to";
            saveFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                var lines = new string[rowCount];
                var rowNum = 0;

                foreach (DataRow row in dataSet.Tables[0].Rows)
                    lines[rowNum++] = string.Join(",", row.ItemArray.ToArray());

                System.IO.File.WriteAllLines(saveFileDialog1.FileName, lines);

                MessageBoxFactory.Show("Successfuly exported " + rowCount + " rows to file: " + saveFileDialog1.FileName, "Export");
            }
        }
    }
}
