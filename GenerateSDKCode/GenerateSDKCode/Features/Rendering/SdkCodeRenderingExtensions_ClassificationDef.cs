using System.CodeDom;
using InRule.Repository.Classifications;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions_ClassificationDef
    {
        public static CodeExpression ToCodeExpression(this ClassificationDef def, CodeTypeDeclarationEx outerClass)
        {
            outerClass.EnsureCreateClassificationDef();
            //public DisplayName
            //public Expression
            //var value = new Func<InRule.Repository.Classifications.ClassificationDef>(() =>
            //{
            //    var ret = new InRule.Repository.Classifications.ClassificationDef();
            //    ret.DisplayName = def.DisplayName;
            //    ret.Expression = def.Expression;
            //    return ret;
            //}).Invoke();
            var method = new CodeMethodReferenceExpression() {MethodName = "CreateClassificationDef"};
            var invoke = new CodeMethodInvokeExpression() {Method = method};
            invoke.Parameters.Add(def.DisplayName.ToCodeExpression());
            invoke.Parameters.Add(def.Expression.ToCodeExpression());
            return invoke;
        }

        public static void EnsureCreateClassificationDef(this CodeTypeDeclarationEx outerClass)
        {
            if (outerClass.ParentClass != null)
            {
                outerClass.ParentClass.EnsureCreateClassificationDef();
                return;
            }

            var methodName = "CreateClassificationDef";
            if (!outerClass.ContainsMethod(methodName))
            {
                var method = new CodeMemberMethodEx(outerClass);
                method.Name = methodName;
                method.Attributes |= MemberAttributes.Static;
                method.ReturnType = typeof(ClassificationDef).ToCodeTypeReference();
                method.Parameters.Add(new CodeParameterDeclarationExpression()
                {
                    Name = "displayName",
                    Type = typeof(string).ToCodeTypeReference()
                });
                method.Parameters.Add(new CodeParameterDeclarationExpression()
                {
                    Name = "expression",
                    Type = typeof(string).ToCodeTypeReference()
                });
                var type = typeof(ClassificationDef);
                var codeTypeReference = type.ToCodeTypeReference();
                var classificationDef = new CodeMethodVariableReferenceEx(method,
                    method.AddMethodVariable("classificationDef", codeTypeReference, type.CallCodeConstructor()));
                classificationDef.SetProperty("DisplayName",
                    new CodeVariableReferenceExpression() {VariableName = "displayName"});
                classificationDef.SetProperty("Expression",
                    new CodeVariableReferenceExpression() {VariableName = "expression"});
                classificationDef.AddAsReturnStatement();
                outerClass.AddMethod(method);
            }
        }
    }
}