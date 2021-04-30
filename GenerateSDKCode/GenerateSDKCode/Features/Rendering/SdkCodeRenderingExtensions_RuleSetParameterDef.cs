using System.CodeDom;
using System.Collections.Generic;
using InRule.Repository;
using InRule.Repository.RuleElements;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
   
    public static partial class SdkCodeRenderingExtensions_RulesetParameterDef
    {

        public static CodeExpression ToCodeExpression(this RuleSetParameterDef def, CodeTypeDeclarationEx outerClass)
        {
            if (def.VersionsSpecified) { return def.RenderGenericDefCreateMethod(outerClass); }

            var method = new CodeMethodReferenceExpression() { MethodName = "CreateRulesetParameterDef" };
            var factoryInvokeMethod = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(outerClass.GetDefFactory().Name), method.MethodName);

            var defProperities = new List<CodeDefPropertyDescriptor>();
            outerClass.AppendFactoryDefPropertySetter(factoryInvokeMethod, defProperities, "Name", def.Name);
            outerClass.AppendFactoryDefPropertySetter(factoryInvokeMethod, defProperities, "DataType", def.DataType);

            if (def.DataType == DataType.Entity)
            {
                SdkCodeRenderingExtensions.AppendFactoryDefPropertySetter(outerClass, factoryInvokeMethod, defProperities, "DataTypeEntityName", def.DataTypeEntityName);
            }

            outerClass.AppendFactoryDefPropertySetter(factoryInvokeMethod, defProperities, "AutoCreate", def.AutoCreate);

            outerClass.EnsureSharedFactoryMethod( def.GetType().ToCodeTypeReference(), method.MethodName, defProperities.ToArray());

            return factoryInvokeMethod;
        }


    }
}

