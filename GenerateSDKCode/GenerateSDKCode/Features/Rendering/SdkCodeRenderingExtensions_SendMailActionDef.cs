using System.CodeDom;
using InRule.Repository.RuleElements;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions_SendMailActionDef
    {
        public static CodeObjectCreateExpression ToCodeExpression(this SendMailActionAttachmentDef def)
        {
            // public SendMailActionAttachmentDef(string argText)

            return typeof(SendMailActionAttachmentDef).CallCodeConstructor(def.ArgText.ToCodeExpression());
        }

    }
}