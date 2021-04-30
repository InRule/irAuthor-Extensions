using System.CodeDom;
using InRule.Repository.DecisionTables;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions_DecisionTableDefs
    {

        public static CodeExpression ToCodeExpression(this ConditionNodeDef def)
        {
            //public ConditionNodeDef(Guid sourceDimensionID, Guid sourceValueID)
            return typeof(ConditionNodeDef).CallCodeConstructor(def.SourceDimensionID.ToCodeExpression(),
                def.SourceValueID.ToCodeExpression());
        }

        public static CodeExpression ToCodeExpression(this ActionNodeDef def)
        {
            //public ActionNodeDef(Guid sourceDimensionID, Guid sourceValueID)
            return typeof(ActionNodeDef).CallCodeConstructor(def.SourceDimensionID.ToCodeExpression(),
                def.SourceValueID.ToCodeExpression());
        }
    }
}