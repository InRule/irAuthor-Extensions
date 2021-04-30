using System.CodeDom;
using InRule.Repository.ValueLists;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions_ValueListItem
    {
        public static CodeExpression ToCodeExpression(this ValueListItemDef valueListItemDef)
        {
            if (valueListItemDef == null) return SdkCodeRenderingExtensions.NullCodeExpression;
            if (valueListItemDef.DisplayText != null)
            {
                return typeof(ValueListItemDef).CallCodeConstructor(valueListItemDef.Value.ToCodeExpression(),
                    valueListItemDef.DisplayText.ToCodeExpression());
            }
            else
            {
                return typeof(ValueListItemDef).CallCodeConstructor(valueListItemDef.Value.ToCodeExpression());
            }
        }
    }
}