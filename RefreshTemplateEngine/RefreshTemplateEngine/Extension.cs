using System;
using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using InRule.Common.Utilities;
using InRule.Repository;

namespace InRule.Authoring.Extensions.RefreshTemplateEngine
{
	public class Extension : ExtensionBase
	{
		private readonly VisualDelegateCommand _refreshTemplateEngineCommand;

		public Extension()
			: base("RefreshTemplateEngine", "Refreshes the template engine", new Guid("{D9DA19AA-2C3E-4510-921B-C87F4FA60BCE}"))
		{
			_refreshTemplateEngineCommand = new VisualDelegateCommand(RefreshTemplateEngine, "Refresh Template Engine", ImageFactory.GetImageAuthoringAssembly("/Images/Refresh32.png"), null, false);
		}

		public override void Enable()
		{
			var ruleAppGroup = IrAuthorShell.HomeTab.GetGroup("Rule Application");
			ruleAppGroup.AddButton(_refreshTemplateEngineCommand);

			RuleApplicationService.RuleApplicationDefChanged += WhenRuleApplicationDefChanged;
		}

		private void WhenRuleApplicationDefChanged(object sender, EventArgs<RuleApplicationDef> e)
		{
			_refreshTemplateEngineCommand.IsEnabled = RuleApplicationService.RuleApplicationDef != null;
		}

		private void RefreshTemplateEngine(object obj)
		{
			var selectedDef = SelectionManager.SelectedItem;
			SelectionManager.SelectedItem = null;

			RuleApplicationService.ResetTemplateEngine(true);

			SelectionManager.SelectedItem = selectedDef;
		}
	}
}
