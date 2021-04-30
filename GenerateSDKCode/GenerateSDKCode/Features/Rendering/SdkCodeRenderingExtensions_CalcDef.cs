using System.CodeDom;
using System.Collections.Generic;
using InRule.Repository;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions
    {
        public static CodeExpression ToCodeExpression(this CalcDef def, CodeTypeDeclarationEx outerClass)
        {
            if (def.VersionsSpecified)
            {
                return def.RenderGenericDefCreateMethod(outerClass);
            }
            var method = new CodeMethodReferenceExpression() {MethodName = "CreateCalcDef"};
            var factoryInvokeMethod =
                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(outerClass.GetDefFactory().Name),
                    method.MethodName);
            
            

            var defProperities = new List<CodeDefPropertyDescriptor>();

            AppendFactoryDefPropertySetter(outerClass, factoryInvokeMethod, defProperities,"FormulaText", def.FormulaText);
            AppendFactoryDefPropertySetter(outerClass, factoryInvokeMethod, defProperities, "ReturnType", def.ReturnType);
            
            if (def.FormulaParseFormatSpecified)
            {
                AppendFactoryDefPropertySetter(outerClass, factoryInvokeMethod, defProperities, "FormulaParseFormat", def.FormulaParseFormat);
            }

            if (def.LastAuthoringViewSpecified)
            {
                AppendFactoryDefPropertySetter(outerClass, factoryInvokeMethod, defProperities, "LastAuthoringView", def.LastAuthoringView);
            }

            EnsureSharedFactoryMethod(outerClass, def.GetType().ToCodeTypeReference(), method.MethodName, defProperities.ToArray());
            
            return   factoryInvokeMethod;
        }

       }
  
}