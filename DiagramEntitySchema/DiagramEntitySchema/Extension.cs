using System;
using System.Collections.Generic;
using InRule.Authoring.Commanding;
using InRule.Authoring.Windows;
using InRule.Repository;
using Newtonsoft.Json;

namespace InRule.Authoring.Extensions.DiagramEntitySchema
{
    public class Extension : ExtensionBase
    {
        private VisualDelegateCommand _submitCommand;

        public Extension()
            : base("Diagram Entity Schema", "Create a diagram representing the Rule App's Entity Schema", new Guid("{10E115A8-C7B4-45EF-AABA-65F15C24E156}"))
        {
            
        }

        public override void Enable()
        {
            var group = IrAuthorShell.HomeTab.GetGroup("Reports");
            var button = group.GetControl("Schema Diagram");
            if (button != null)
                group.RemoveItem(button);
            _submitCommand = new VisualDelegateCommand(Submit, "Schema Diagram", "/Images/flow32.png", "/Images/flow32.png", false);
            group.AddButton(_submitCommand);
            _submitCommand.IsEnabled = RuleApplicationService.RuleApplicationDef != null;

            RuleApplicationService.Opened += WhenRuleApplicationChanged;
            RuleApplicationService.Closed += WhenRuleApplicationChanged;
        }

        private void WhenRuleApplicationChanged(object sender, EventArgs e)
        {
            var enabled = RuleApplicationService.RuleApplicationDef != null;
            _submitCommand.IsEnabled = enabled;
        }

        private void Submit(object obj)
        {
            try
            {
                RuleApplicationDef ruleApp = RuleApplicationService.RuleApplicationDef;
                var jsonData = GetRuleAppEntityStructure(ruleApp);

                string htmlContent = DiagramEntitySchema.Properties.Resources.SchemaDiagram;
                htmlContent = htmlContent.Replace("{ENTITYSTRUCTURE}", jsonData);

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
