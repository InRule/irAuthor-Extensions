using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using InRule.Repository;
using InRuleLabs.AuthoringExtensions.FieldsInUse.Extensions;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Views
{
    /// <summary>
    /// Interaction logic for FieldUsageSummary.xaml
    /// </summary>
    public partial class FieldUsageSummary : Window
    {
        public FieldUsageSummary()
        {
            InitializeComponent();
        }

        public void Populate(InRule.Repository.RuleApplicationDef ruleAppDef)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"----- Field Usage Summary -----");

            sb.AppendLine();
            sb.AppendLine(BuildFieldList($"UNUSED FIELDS", "(Unused)", ruleAppDef.GetUnusedFields()));

            sb.AppendLine();
            sb.AppendLine(BuildFieldList($"CONSUMED FIELDS", "(Consumed)" , ruleAppDef.GetConsumedFields().Select(t=>t.TargetDef).ToList()));

            sb.AppendLine();
            sb.AppendLine(BuildFieldList($"UPDATED FIELDS", "(Updated)", ruleAppDef.GetUpdatedFields().Select(t => t.TargetDef).ToList()));

            this.txtSummary.Text = sb.ToString();
        }

        private static string BuildFieldList(string title, string prefix, List<RuleRepositoryDefBase> elementList)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);
            foreach (var fld in elementList.OrderBy(t=>t.AuthoringElementPath))
            {
                sb.AppendLine($"{prefix} {fld.AuthoringElementPath}");
            }
            return sb.ToString();
        }
    }
}
