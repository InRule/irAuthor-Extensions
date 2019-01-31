using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InRule.Authoring.Windows;
using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Repository;
using InRule.Repository.Infos;

namespace irXFindUnusedSchema
{
    public class Extension : ExtensionBase
    {
        public const string ExtensionShortName = "FindUnusedSchema";
        public const string ExtensionFullName = "irX Find Unused Fields in Schema";
        public const string ExtensionButtonText = "Find Unused Schema";
        public const string ExtensionDescription = "This control will identify field elements in the schema that are not used!";
        public const string ExtensionRibbonGroup = "mwc";
        public const string ExtensionImageSource16 = "/Images/UserDefinedFunctionLibrary16.png";
        public const string ExtensionImageSource32 = "/Images/UserDefinedFunctionLibrary32.png";

        public const string ExtensionGuid = "{AB38D449-0C9D-41CE-BEB5-07474D705612}";

        public static Guid ID = new Guid(ExtensionGuid);

        public VisualDelegateCommand __ExtensionCommand;

        public Extension() : base(ExtensionShortName, ExtensionDescription, ID) { }

        public override void Enable()
        {
            var group = IrAuthorShell.HomeTab.AddGroup(ExtensionRibbonGroup, ImageFactory.GetImageAuthoringAssembly(ExtensionImageSource16));
            __ExtensionCommand = new VisualDelegateCommand(ButtonWasPressed, ExtensionButtonText, ImageFactory.GetImageAuthoringAssembly(ExtensionImageSource16), ImageFactory.GetImageAuthoringAssembly(ExtensionImageSource32), false);
            group.AddButton(__ExtensionCommand);
            RuleApplicationService.Opened += SetEnabled;
            RuleApplicationService.Closed += SetEnabled;
            SelectionManager.SelectedItemChanged += SetEnabled;
        }

        private void SetEnabled(object sender, EventArgs e)
        {
            __ExtensionCommand.IsEnabled = true;
        }



        private void ButtonWasPressed(object obj)
        {
            StringBuilder sb = new StringBuilder();
            var ra = base.RuleApplicationService.RuleApplicationDef;
            // var ra = new RuleApplicationDef();
            // var e1Def = ra.Entities.Add(new EntityDef("e1"));
            // var f1Def = e1Def.Fields.Add(new FieldDef("f1", DataType.Integer));
            // var f2Def = e1Def.Fields.Add(new FieldDef("f2", DataType.Integer));
            // var f3Def = e1Def.Fields.Add(new FieldDef("f3", DataType.Integer));
            // var calc1Def = e1Def.Fields.Add(new FieldDef("calc1", DataType.Integer) { IsCalculated = true });
            // calc1Def.Calc.FormulaText = $"{f1Def.Name} + {f2Def.Name}";

            sb.AppendLine("Finding unused FieldDefs...");
            DefUsageNetwork usages = DefUsageNetwork.Create(ra);

            List<FieldDef> allFields = ra.GetChildDefsByType<FieldDef>();
            foreach (FieldDef field in allFields)
            {
                if (!usages.GetDefUsages(field.Guid).Any(usage => usage.UsageType == DefUsageType.ConsumedBy
                                                                || usage.UsageType == DefUsageType.InvalidatedBy
                                                                || usage.UsageType == DefUsageType.UpdatedBy))
                {
                    sb.AppendLine($"Field: {field.AuthoringElementPath} is unused.");
                }
            }

            DisplayForm form = new DisplayForm();
            form.TheText = sb.ToString();
            form.Show();

        }


    }
}
