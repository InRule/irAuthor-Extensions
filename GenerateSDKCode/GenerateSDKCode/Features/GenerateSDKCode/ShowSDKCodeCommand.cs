using InRule.Authoring.Commands;
using InRule.Authoring.Windows;
using InRule.Repository;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.GenerateSDKCode
{
    public class ShowSDKCodeCommand : DefCommandBase
    {

        public IIrAuthorShell IrAuthorShell { get; set; }

        public ShowSDKCodeCommand(RuleRepositoryDefBase def) : base("ShowSDKCode", "Generate SDK Code", "", def)
        {
            this.Label = "Show SDK Code for " + Def.GetType().Name;
        }

        public override void Execute()
        {
            // copy defs in case they are used by other threads
            var win = new SDKCodePopupView(Def);
            win.Show();
        }
    }
   
}
