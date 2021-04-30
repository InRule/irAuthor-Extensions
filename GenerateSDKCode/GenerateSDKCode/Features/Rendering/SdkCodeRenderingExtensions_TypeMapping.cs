using System;
using System.CodeDom;
using System.Linq;
using InRule.Repository.RuleElements;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions_TypeMapping
    {

        public static CodeExpression ToCodeExpression(this TypeMapping typeMapping, CodeTypeDeclarationEx outerClass)
        {
            //public TypeMapping(string name, string dataType, Type baseType, Type[] derivedTypes)
            CodeExpression baseType = ((Type)null).ToCodeExpression2(outerClass);
            CodeExpression[] derivedTypes = new CodeExpression[0];
            if (typeMapping.BaseType != null)
            {
                baseType = new CodeMethodInvokeExpression(new CodeSnippetExpression("System.Type"), "GetType", typeMapping.BaseType.FullName.ToCodeExpression());
            }
            if (typeMapping.DerivedTypes != null)
            {
                derivedTypes =
                    typeMapping.DerivedTypes.Select(
                        t =>
                            (CodeExpression)new CodeMethodInvokeExpression(new CodeSnippetExpression("System.Type"), "GetType",
                                SdkCodeRenderingExtensions.ToCodeExpression2(t.FullName, outerClass))).ToArray();
            }
            var derivedTypesArray = new CodeArrayCreateExpression(new CodeTypeReference(typeof(Type)), derivedTypes);

            return typeof(TypeMapping).CallCodeConstructor(
                typeMapping.Name.ToCodeExpression()
                , typeMapping.DataType.ToCodeExpression()
                , baseType
                , derivedTypesArray
                );
        }
    }
}