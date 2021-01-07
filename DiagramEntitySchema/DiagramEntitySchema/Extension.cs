using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using InRule.Authoring.Commanding;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Controls;
using InRule.Repository;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace InRule.Authoring.Extensions.DiagramEntitySchema
{
    public class Extension : ExtensionBase
    {
        private VisualDelegateCommand _loadCommand;
        private VisualDelegateCommand _localCommand;
        private VisualDelegateCommand _ffCommand;
        private VisualDelegateCommand _browserCommand;

        public Extension()
            : base("Diagram Entity Schema", "Create a diagram representing the Rule App's Entity Schema", new Guid("{10E115A8-C7B4-45EF-AABA-65F15C24E156}"))
        {
        }

        public override void Enable()
        {
            var group = IrAuthorShell.HomeTab.GetGroup("Reports");

            var existingButton = group.GetControl("Schema Diagram");
            if (existingButton != null)
                group.RemoveItem(existingButton);

            _loadCommand = new VisualDelegateCommand(LoadDiagram, "Schema Diagram", "/Images/flow32.png", "/Images/flow32.png", false);
            _loadCommand.IsEnabled = RuleApplicationService.RuleApplicationDef != null;

            _localCommand = new VisualDelegateCommand(LoadDiagram, "Open in Local Navigator", "/Images/flow16.png", "/Images/flow32.png");
            _localCommand.IsEnabled = RuleApplicationService.RuleApplicationDef != null;

            _ffCommand = new VisualDelegateCommand(OpenInFF, "Launch in Firefox", "/Images/flow16.png", "/Images/flow32.png");
            _ffCommand.IsEnabled = IsFirefoxInstalled() && RuleApplicationService.RuleApplicationDef != null;

            _browserCommand = new VisualDelegateCommand(OpenInBrowser, "Launch in Default Browser", "/Images/flow16.png", "/Images/flow32.png");
            _browserCommand.IsEnabled = RuleApplicationService.RuleApplicationDef != null;

            IRibbonMenuButton diagramButton = group.AddSplitMenuButton(_loadCommand);
            diagramButton.AddMenuItem(_localCommand);
            diagramButton.AddMenuItem(_ffCommand);
            diagramButton.AddMenuItem(_browserCommand);

            RuleApplicationService.Opened += WhenRuleApplicationChanged;
            RuleApplicationService.Closed += WhenRuleApplicationChanged;
        }

        private void WhenRuleApplicationChanged(object sender, EventArgs e)
        {
            var enabled = RuleApplicationService.RuleApplicationDef != null;
            _loadCommand.IsEnabled = enabled;
            _localCommand.IsEnabled = enabled;
            _ffCommand.IsEnabled = IsFirefoxInstalled() && enabled;
            _browserCommand.IsEnabled = enabled;
        }
        
        private void LoadDiagram(object obj)
        {
            try
            {
                var htmlContent = GetDiagramHtml();

                var reportWindow = new ReportWindow(RuleApplicationService, SelectionManager);
                reportWindow.webControl.Loaded += delegate
                {
                    reportWindow.webControl.NavigateToString(htmlContent);
                };
                reportWindow.Title = "Entity Schema Diagram";
                reportWindow.Show();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        private void OpenInFF(object obj)
        {
            System.Diagnostics.Process.Start("firefox.exe", WriteToTempFile());
        }
        private void OpenInBrowser(object obj)
        {
            System.Diagnostics.Process.Start(WriteToTempFile());
        }





        // Helpers
        private string GetDiagramHtml()
        {
            RuleApplicationDef ruleApp = RuleApplicationService.RuleApplicationDef;
            var jsonData = GetRuleAppEntityStructure(ruleApp);

            string htmlContent = DiagramEntitySchema.Properties.Resources.SchemaDiagram;
            htmlContent = htmlContent.Replace("{ENTITYSTRUCTURE}", jsonData);

            return htmlContent;
        }
        private string WriteToTempFile()
        {
            var outputPath = Path.Combine(Path.GetTempPath(), $"{RuleApplicationService.RuleApplicationDef.Name} Schema Diagram {DateTime.Now.ToShortDateString().Replace("/", "-")} {DateTime.Now.ToShortTimeString().Replace(":", ".")}.html");
            File.WriteAllText(outputPath, GetDiagramHtml());
            return $"\"{outputPath}\"";
        }
        public static bool IsFirefoxInstalled()
        {
            RegistryKey firefixDir = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Mozilla\Mozilla Firefox", false);
            return firefixDir != null;
        }

        private string GetRuleAppEntityStructure(RuleApplicationDef def)
        {
            var entities = new List<entityBasicInfo>();
            foreach (EntityDef entity in def.Entities)
            {
                var ent = new entityBasicInfo()
                {
                    name = entity.Name,
                    fields = new List<fieldBasicInfo>()
                };
                foreach (FieldDef field in entity.Fields)
                {
                    ent.fields.Add(new fieldBasicInfo()
                    {
                        name = field.Name,
                        isTemporary = field.StateLocation == StateLocation.TemporaryState,
                        isCollection = field.IsCollection,
                        dataType = field.DataType.ToString(),
                        entityName = field.DataTypeEntityName
                    });
                }
                entities.Add(ent);
            }
            var json = JsonConvert.SerializeObject(entities);
            return json;
        }
        private class entityBasicInfo
        {
            public string name;
            public List<fieldBasicInfo> fields;
        }
        private class fieldBasicInfo
        {
            public string name;
            public bool isTemporary;
            public bool isCollection;
            public string dataType;
            public string entityName;
        }
    }
}
