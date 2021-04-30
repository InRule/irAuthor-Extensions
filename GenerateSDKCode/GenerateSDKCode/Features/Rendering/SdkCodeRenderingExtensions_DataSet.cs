using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions_DataSet
    {
        #region DataSet and DataTable
        // Data Helpers
        public static CodeExpression ToCodeExpression(this DataSet dataSet, CodeTypeDeclarationEx outerClass)
        {
            if (dataSet == null) return SdkCodeRenderingExtensions.NullCodeExpression;

            var createMethod = outerClass.AddCreateMethod(dataSet);
            var target = createMethod.AddReturnVariable("def", dataSet.GetType().CallCodeConstructor());
            foreach (DataTable tbl in dataSet.Tables)
            {
                target.GetPropertyReference("Tables").AddInvokeMethodStatement("Add", tbl.ToCodeExpression(outerClass));
            }
            target.SetProperty("DataSetName", dataSet.DataSetName.ToCodeExpression());
            target.AddAsReturnStatement();
            return new CodeMethodInvokeExpression(null, createMethod.Name);
        }
        public static CodeExpression ToCodeExpression(this DataTable dataTable, CodeTypeDeclarationEx outerClass)
        {
            var createMethod = outerClass.AddCreateMethod(dataTable);
            var target = createMethod.AddReturnVariable("def", dataTable.GetType().CallCodeConstructor());
            target.SetProperty("TableName", dataTable.TableName.ToCodeExpression());
            target.SetProperty("DisplayExpression", dataTable.DisplayExpression.ToCodeExpression());

            if (dataTable.Columns.Count > 0)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    var type = col.DataType;
                    var name = col.ColumnName;
                    //var col = new DataColumn(columnName, Type);
                    var method = new CodeMethodInvokeExpression(new CodeSnippetExpression("System.Type"), "GetType", type.FullName.ToCodeExpression());
                    var createCol = typeof(DataColumn).CallCodeConstructor(name.ToCodeExpression(), method);
                    target.GetPropertyReference("Columns").AddInvokeMethodStatement("Add", createCol);
                }
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var cellValues = new List<CodeExpression>();
                        foreach (var item in row.ItemArray)
                        {
                            cellValues.Add(item.ToCodeExpression2(outerClass));
                        }
                        target.GetPropertyReference("Rows").AddInvokeMethodStatement("Add", cellValues.ToArray());
                    }

                }
            }
            target.AddAsReturnStatement();
            return new CodeMethodInvokeExpression(null, createMethod.Name);
        }
        #endregion
        // String Array
        public static CodeExpression ToCodeExpression(this string[] value)
        {
            if (value == null) return SdkCodeRenderingExtensions.NullCodeExpression;
            return new CodeArrayCreateExpression(typeof(string).ToCodeTypeReference(), value.Select(item => SdkCodeRenderingExtensions_Primitives.ToCodeExpression((string) item)).ToArray());
        }
    }
}