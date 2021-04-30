using System.CodeDom;
using InRule.Repository;
using InRule.Repository.RuleElements;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions_DataDefs
    {

        public static CodeExpression ToCodeExpression(this NameSortOrderDef def)
        {
            // public NameSortOrderDef(string name, SortOrder sortOrder) : base(name)

            return typeof(NameSortOrderDef).CallCodeConstructor(def.Name.ToCodeExpression(),
                def.SortOrder.ToCodeExpression());
        }

        public static CodeExpression ToCodeExpression(this ExecuteSqlQueryActionParameterValueDef def)
        {
            //public ExecuteSqlQueryActionParameterValueDef(string argName, string argExpression, DataType paramType, SqlQueryParmDef.ParameterDirection parameterDirection)

            return typeof(ExecuteSqlQueryActionParameterValueDef).CallCodeConstructor(
                def.Name.ToCodeExpression()
                , def.ArgValue.FormulaText.ToCodeExpression()
                , def.ParamType.ToCodeExpression()
                , def.ParameterDirection.ToCodeExpression()
                );
        }

        public static CodeExpression ToCodeExpression(this NameExpressionPairDef def)
        {
            //public NameExpressionPairDef(string name, string expression)
            return typeof(NameExpressionPairDef).CallCodeConstructor(def.Name.ToCodeExpression(),
                def.ExpressionText.ToCodeExpression());
        }
    }
}