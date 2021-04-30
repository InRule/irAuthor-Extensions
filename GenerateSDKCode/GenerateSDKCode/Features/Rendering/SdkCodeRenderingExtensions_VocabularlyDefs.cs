using System.CodeDom;
using InRule.Repository.Vocabulary;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{

    public static partial class SdkCodeRenderingExtensions_VocabularlyDefs
    {
        public static CodeExpression ToCodeExpression(this ExecuteRulesetParameterDef def)
        {
            if (def == null) return SdkCodeRenderingExtensions.NullCodeExpression;
            {
                return typeof(ExecuteRulesetParameterDef).CallCodeConstructor(def.FieldName.ToCodeExpression(),
                    def.FieldValuePrototype.ToCodeExpression());
            }
        }
        public static CodeExpression ToCodeExpression(this EntityValueDef def)
        {
            if (def == null) return SdkCodeRenderingExtensions.NullCodeExpression;
            {
                return typeof(EntityValueDef).CallCodeConstructor(def.EntityName.ToCodeExpression());
            }
        }
        public static CodeExpression ToCodeExpression(this DataTypeValueDef def)
        {
            if (def == null) return SdkCodeRenderingExtensions.NullCodeExpression;
            {
                return typeof(DataTypeValueDef).CallCodeConstructor(def.DataType.ToCodeExpression());
            }
        }

    }
}