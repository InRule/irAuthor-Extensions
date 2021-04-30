using System.CodeDom;
using InRule.Repository.RuleElements;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions_ExecuteMethodActionParamDef
    {

        public static CodeExpression ToCodeExpression(this ExecuteMethodActionParamDef def)
        {
            // public ExecuteMethodActionParamDef(string argExpression, string argName, string fullTypeName, string fullDisplayTypeName, string assemblyName, bool isOut)
            return typeof(ExecuteMethodActionParamDef).CallCodeConstructor(
                def.ArgExpression.ToCodeExpression()
                , def.ArgName.ToCodeExpression()
                , def.FullTypeName.ToCodeExpression()
                , def.FullDisplayTypeName.ToCodeExpression()
                , def.AssemblyName.ToCodeExpression()
                , def.IsOut.ToCodeExpression()
                );
        }
    }
}