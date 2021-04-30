using System;
using System.CodeDom;
using System.Linq;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public class CodeMethodVariableReferenceEx
    {
        public CodeMemberMethodEx Method;
        public CodeExpression Variable;
        public CodeTypeDeclarationEx OuterClass { get { return this.Method.OuterClass; } }
        public string VariableName;
        public CodeMethodVariableReferenceEx(CodeMemberMethodEx method, CodeExpression variable)
        {
            Method = method;
            Variable = variable;
        }
        public CodeMethodVariableReferenceEx(CodeMemberMethodEx method, CodeVariableReferenceExpression variable) : this(method, (CodeExpression)variable)
        {
            VariableName = variable.VariableName;
        }

        public void SetProperty(string name, CodeExpression value)
        {
            Method.AddSetProperty(Variable, name, value);
        }
        //public void SetProperty(string name, string value)
        //{
        //    Method.AddSetProperty(Variable, name, value.ToCodeExpression(this.OuterClass));
        //}
        //public void SetProperty(string name, Guid value)
        //{
        //    Method.AddSetProperty(Variable, name, value.ToCodeExpression(this.OuterClass));
        //}

        //public void SetProperty(string name, ValueType value)
        //{
        //    Method.AddSetProperty(Variable, name, value.ToCodeExpression(this.OuterClass));
        //}
        //public void SetNameProperty(string value)
        //{
        //    SetProperty("Name", value);
        //}
        public CodeMethodVariableReferenceEx GetPropertyReference(string entities)
        {
            return new CodeMethodVariableReferenceEx(this.Method, new CodePropertyReferenceExpression(Variable, entities));
        }

        public void AddInvokeMethodStatement(string methodName, params CodeExpression[] parms)
        {
            var invokeMethod = new CodeMethodInvokeExpression(this.Variable, methodName, parms);
            Method.Statements.Add(invokeMethod);
        }
        public void AddAsReturnStatement()
        {
            Method.AddReturnStatment(Variable);
        }

        public void AddLiteralStatement(string literal)
        {
            Method.Statements.Add(new CodeSnippetStatement() { Value = literal });
        }

        public CodeMethodVariableReferenceEx DeclareVariable(string variableName, Type type, CodeExpression value)
        {
            var ret = this.Method.AddMethodVariable(variableName, type, value);
            return new CodeMethodVariableReferenceEx(this.Method, ret);
        }

        public CodeMethodVariableReferenceEx GetIndexer(params CodeExpression[] indicies)
        {
            var ret = new CodeIndexerExpression(this.Variable, indicies);
            return new CodeMethodVariableReferenceEx(this.Method, ret);
        }
        public CodeMethodVariableReferenceEx GetIndexer(params CodeMethodVariableReferenceEx[] indicies)
        {
            var ret = new CodeIndexerExpression(this.Variable, indicies.Select(i => i.Variable).ToArray());
            return new CodeMethodVariableReferenceEx(this.Method, ret);
        }

        //public void SetIndexerValue(string key, string value)
        //{
        //    var ret = new CodeIndexerExpression(this.Variable, key.ToCodeExpression(this.OuterClass));
        //    var setProp = new CodeAssignStatement(ret, value.ToCodeExpression(this.OuterClass));
        //    this.Method.Statements.Add(setProp);
        //}
        public void SetIndexerValue(CodeExpression key, CodeExpression value)
        {
            var ret = new CodeIndexerExpression(this.Variable, key);
            var setProp = new CodeAssignStatement(ret, value);
            this.Method.Statements.Add(setProp);
        }
        //public void AddSetValueIfNotEqualTo(CodeTypeDeclarationEx outerClass, object sourceDef, PropertyInfo propInfo, string value)
        //{
        //    var propertyValue = (String)propInfo.GetValue(sourceDef);
        //    if (propertyValue != value)
        //    {
        //        AddSetValueFromProperty(outerClass, sourceDef, propInfo);
        //    }
        //}



        //public void AddSetValueIfSpecified(CodeTypeDeclarationEx outerClass, object sourceDef, PropertyInfo propInfo)
        //{

        //    var target = this;
        //    var specifiedName = propInfo.Name + "Specified";
        //    var specifiedProperty = sourceDef.GetType().GetProperty(specifiedName);
        //    if (specifiedProperty == null || (bool)specifiedProperty.GetValue(sourceDef))
        //    {
        //        AddSetValueFromProperty(outerClass, sourceDef, propInfo);
        //    }
        //}
        ////public void AddSetValueIfSpecified(CodeTypeDeclarationEx outerClass, object sourceDef, FieldInfo propInfo)
        //{

        //    var target = this;
        //    var specifiedName = propInfo.Name + "Specified";
        //    var specifiedProperty = sourceDef.GetType().GetProperty(specifiedName);
        //    if (specifiedProperty == null || (bool)specifiedProperty.GetValue(sourceDef))
        //    {
        //        AddSetValueFromProperty(outerClass, sourceDef, propInfo);
        //    }
        //}
        //public void AddSetValueFromProperty(CodeTypeDeclarationEx outerClass, object sourceDef, PropertyInfo propInfo)
        //{
        //    try
        //    {
        //        AddSetValueFromProperty_Internal(outerClass, propInfo.Name, propInfo.PropertyType, new Func<object>(() => propInfo.GetValue(sourceDef)));
        //    }
        //    catch (Exception ex)
        //    {
        //        var propValue = ex.Message;
        //        this.AddLiteralStatement(String.Format("{0}.UNHANDLED1(\"{1}\", {2});",
        //                this.VariableName,
        //                propInfo.Name, propValue));
        //    }
        //}
        //public void AddSetValueFromProperty(CodeTypeDeclarationEx outerClass, object sourceDef, FieldInfo propInfo)
        //{
        //    try
        //    {
        //        AddSetValueFromProperty_Internal(outerClass, propInfo.Name, propInfo.FieldType, new Func<object>(() => propInfo.GetValue(sourceDef)));
        //    }
        //    catch (Exception ex)
        //    {
        //        var propValue = ex.Message;
        //        this.AddLiteralStatement(String.Format("{0}.UNHANDLED2(\"{1}\", {2});",
        //                this.VariableName,
        //                propInfo.Name, propValue));
        //    }
        //}
        //private void AddSetValueFromProperty_Internal(CodeTypeDeclarationEx outerClass, string propertyName, Type propertyType, Func<object> getValue)
        //{
        //    var target = this;
        //    if (propertyType == typeof(object) || propertyType.IsAbstract)
        //    {
        //        //When object try and get the actual value to get the type and call recursivly
        //        var value = getValue();
        //        if (value == null)
        //        {
        //            return;
        //        }
        //        AddSetValueFromProperty_Internal(outerClass, propertyName, value.GetType(), new Func<object>(() => value));
        //        return;

        //    }
        //    if (propertyType == typeof(int))
        //    {
        //        target.SetProperty(propertyName, (int)getValue());
        //    }
        //    else if (propertyType == typeof(string))
        //    {
        //        target.SetProperty(propertyName, (string)getValue());
        //    }
        //    else if (propertyType == typeof(bool))
        //    {
        //        target.SetProperty(propertyName, (bool)getValue());
        //    }
        //    else if (propertyType == typeof(DateTime))
        //    {
        //        target.SetProperty(propertyName, ((DateTime)getValue()).ToCodeExpression(this.OuterClass));
        //    }
        //    else if (propertyType == typeof(long))
        //    {
        //        target.SetProperty(propertyName, (long)getValue());
        //    }
        //    else if (propertyType == typeof(Guid))
        //    {
        //        target.SetProperty(propertyName, (Guid)getValue());
        //    }
        //    else if (typeof(Enum).IsAssignableFrom(propertyType))
        //    {
        //        target.SetProperty(propertyName, (Enum)getValue());
        //    }
        //    else if (propertyType == typeof(ValueListReferenceDef))
        //    {
        //        var typedPropValue = (ValueListReferenceDef)getValue();
        //        if (typedPropValue != null)
        //        {
        //            var factoryMethod = RenderValueListReferenceDef.Render(outerClass, (ValueListReferenceDef)getValue());
        //            target.SetProperty(propertyName, factoryMethod);
        //        }
        //    }
        //    else if (propertyType == typeof(VocabularyDef))
        //    {
        //        var factoryMethod = RenderVocabularyDef.Render(outerClass, (VocabularyDef)getValue());
        //        target.SetProperty(propertyName, factoryMethod);
        //    }
        //    else if (propertyType == typeof(CalcDef))
        //    {
        //        if (getValue() != null)
        //        {
        //            var typedPropValue = (CalcDef)getValue();
        //            if (!string.IsNullOrEmpty(typedPropValue.FormulaText))
        //            {
        //                var factoryMethod = RenderCalcDef.Render(outerClass, (CalcDef)getValue());
        //                target.SetProperty(propertyName, factoryMethod);
        //            }
        //        }
        //    }
        //    else if (propertyType == typeof(StringCollection))
        //    {
        //        var typedPropValue = (StringCollection)getValue();
        //        var currValue = getValue();
        //        if (currValue == null)
        //        {
        //            target.SetProperty(propertyName, typeof(StringCollection).CallCodeConstructor());
        //        }
        //        var collection = target.GetPropertyReference(propertyName);

        //        List<CodeExpression> vals = new List<CodeExpression>();
        //        foreach (var val in typedPropValue)
        //        {
        //            vals.Add(val.ToCodeExpression(this.OuterClass));
        //        }
        //        var array = new CodeArrayCreateExpression(typeof(String[]).ToCodeTypeReference(), vals.ToArray());
        //        collection.AddInvokeMethodStatement("AddRange", array);
        //    }
        //    else if (TryRenderCollectionProperty<InRule.Repository.Constraints.FieldConstraintDefCollection, InRule.Repository.Constraints.FieldConstraintDef>(RenderFieldConstraintDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<RuleElementDefCollection, RuleElementDef>(RenderRuleElementDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<ExecuteActionParamDefCollection, ExecuteActionParamDef>(RenderExecuteActionParamDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<FieldDefCollection, FieldDef>(RenderFieldDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<DataElementDefCollection, DataElementDef>(RenderDataElementDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<CascadedReferenceDefCollection, CascadedReferenceDef>(RenderCascadedReferenceDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (propertyType == typeof(InRule.Repository.RuleElements.RuleElementDef))
        //    {
        //        var typedPropValue = (InRule.Repository.RuleElements.RuleElementDef)getValue();
        //        if (typedPropValue != null)
        //        {
        //            target.SetProperty(propertyName, RenderRuleElementDef.Render(outerClass, typedPropValue));
        //        }
        //    }
        //    else if (TryRenderCollectionProperty<ValueListItemDefCollection, ValueListItemDef>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<InRule.Repository.Utilities.ExpressionDictionary, KeyValuePair<string, string>>((outer, def) => def.Key.ToCodeExpression(this.OuterClass), (outer, def) => def.Value.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<TemplateAvailabilitySettingCollection, TemplateAvailabilitySetting>(RenderTemplateAvailability.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<TemplateDefCollection, TemplateDef>(RenderTemplateDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<ImpersonationDefCollection, ImpersonationDef>(RenderImpersonationDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<TemplatePlaceholderDefCollection, TemplatePlaceholderDef>(RenderTemplatePlaceholderDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<PlaceholderValueDefCollection, PlaceholderValueDef>(RenderPlaceholderValueDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<CollectionMemberValueAssignmentDefCollection, CollectionMemberValueAssignmentDef>(RenderCollectionMemberValueAssignmentDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<EndPointDefCollection, EndPointDef>(RenderEndPointDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<SqlQueryParmDefCollection, SqlQueryParmDef>(RenderSqlQueryParmDef.Render, outerClass, propertyName, propertyType, getValue)) { }

        //    // Decision Tables
        //    else if (TryRenderCollectionProperty<ConditionDimensionDefCollection, ConditionDimensionDef>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<ActionDimensionDefCollection, ActionDimensionDef>(RenderActionDimensionDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<DecisionDefCollection, DecisionDef>(RenderDecisionDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<ConditionValueDefCollection, ConditionValueDef>(RenderConditionValueDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<ActionValueDefCollection, ActionValueDef>(RenderActionValueDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<ConditionNodeDefCollection, ConditionNodeDef>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<ActionNodeDefCollection, ActionNodeDef>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }

        //    else if (TryRenderCollectionProperty<InRule.Repository.Utilities.ExpressionDictionary, KeyValuePair<string, string>>((outer, def) => def.Key.ToCodeExpression(this.OuterClass), (outer, def) => def.Value.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }

        //    // Data
        //    else if (TryRenderCollectionProperty<ExecuteSqlQueryActionParameterValueDefCollection, ExecuteSqlQueryActionParameterValueDef>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderProperty<TableSettings>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }
        //    else if (propertyType == typeof(InRule.Repository.SqlQuerySettings))
        //    {
        //        var typedPropValue = (InRule.Repository.SqlQuerySettings)getValue();
        //        if (typedPropValue != null)
        //        {
        //            target.SetProperty(propertyName, RenderSqlQuerySettings.Render(outerClass, typedPropValue));
        //        }
        //    }


        //    else if (TryRenderCollectionProperty<XmlSerializableStringDictionary, XmlSerializableStringDictionary.XmlSerializableStringDictionaryItem>((outer, def) => typeof(XmlSerializableStringDictionary.XmlSerializableStringDictionaryItem)
        //                        .CallCodeConstructor(def.Key.ToCodeExpression(outerClass), def.Value.ToCodeExpression(outerClass)), outerClass, propertyName, propertyType, getValue)) { }


        //    // Rules
        //    else if (TryRenderCollectionProperty<NameExpressionPairDefCollection, NameExpressionPairDef>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<ExecuteMethodActionParamDefCollection, ExecuteMethodActionParamDef>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<SendMailActionAttachmentDefCollection, SendMailActionAttachmentDef>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }
        //    else if (TryRenderCollectionProperty<InRule.Repository.Classifications.ClassificationDefCollection, InRule.Repository.Classifications.ClassificationDef>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }


        //    // Schema
        //    else if (TryRenderCollectionProperty<TypeMappingCollection, TypeMapping>((outer, def) => def.ToCodeExpression(outerClass), outerClass, propertyName, propertyType, getValue)) { }

        //    // Vocabulary
        //    else if (TryRenderCollectionProperty<NameSortOrderDefCollection, NameSortOrderDef>((outer, def) => def.ToCodeExpression(this.OuterClass), outerClass, propertyName, propertyType, getValue)) { }




        //    else if (propertyType == typeof(RuleRepositoryDefBase.DefMetadataCollectionCollection))
        //    {
        //        SetPropertyForAttributes(target, outerClass, getValue());
        //    }
        //    else if (propertyType == typeof(DataSet))
        //    {
        //        var typedPropValue = (DataSet)getValue();
        //        if (typedPropValue != null)
        //        {
        //            if (typedPropValue.Tables.Count > 0)
        //            {
        //                target.SetProperty(propertyName, RenderDataSet.Render(outerClass, typedPropValue));
        //            }
        //        }
        //    }
        //    else if (propertyType == typeof(DataTable))
        //    {
        //        var typedPropValue = (DataTable)getValue();
        //        if (typedPropValue != null)
        //        {
        //            target.SetProperty(propertyName, RenderDataTable.Render(outerClass, typedPropValue));
        //        }
        //    }
        //    else if (typeof(InRule.Repository.RuleElements.RuleElementDef).IsAssignableFrom(propertyType))
        //    {
        //        var typedPropValue = (RuleElementDef)getValue();
        //        if (typedPropValue != null)
        //        {
        //            target.SetProperty(propertyName, RenderRuleElementDef.Render(outerClass, typedPropValue));
        //        }
        //    }
        //    else if (typeof(XmlDocumentSettings).IsAssignableFrom(propertyType))
        //    {
        //        var typedPropValue = (XmlDocumentSettings)getValue();
        //        if (typedPropValue != null)
        //        {
        //            target.SetProperty(propertyName, RenderXmlDocumentSettings.Render(outerClass, typedPropValue));
        //        }
        //    }
        //    else if (typeof(XPathQuerySettings).IsAssignableFrom(propertyType))
        //    {
        //        var typedPropValue = (XPathQuerySettings)getValue();
        //        if (typedPropValue != null)
        //        {
        //            target.SetProperty(propertyName, RenderXPathQuerySettings.Render(outerClass, typedPropValue));
        //        }
        //    }
        //    else if (typeof(string[]).IsAssignableFrom(propertyType))
        //    {
        //        var typedPropValue = (string[])getValue();
        //        if (typedPropValue != null)
        //        {
        //            target.SetProperty(propertyName, typedPropValue.ToCodeExpression(outerClass));
        //        }
        //    }

        //    else if (this.TryRenderCollectionProperty<InRule.Repository.EntityDefInfoCollection, InRule.Repository.EntityDefInfo>(RenderAssemblyDefInfo.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (this.TryRenderCollectionProperty<InRule.Repository.EndPoints.AssemblyDef.ClassInfoCollection, InRule.Repository.EndPoints.AssemblyDef.ClassInfo>(RenderAssemblyDefInfo.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (this.TryRenderCollectionProperty<InRule.Repository.WebServiceDef.OperationDefCollection, InRule.Repository.WebServiceDef.OperationDef>(RenderWebServiceDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (this.TryRenderCollectionProperty<InRule.Repository.WebServiceDef.ServicesDefCollection, InRule.Repository.WebServiceDef.ServicesDef>(RenderWebServiceDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (this.TryRenderCollectionProperty<ExecuteRulesetParameterDefCollection, ExecuteRulesetParameterDef>(RenderVocabularyElementDef.Render, outerClass, propertyName, propertyType, getValue)) { }
        //    else if (this.TryRenderCollectionProperty<InRule.Repository.UDFs.UdfLibraryDefCollection, InRule.Repository.UDFs.UdfLibraryDef>(RenderUdfLibraryDef.Render, outerClass, propertyName, propertyType, getValue)) { }


        //    else if (propertyType == typeof(InRule.Repository.XmlQualifiedNameInfo[]))
        //    {
        //        var typedPropValue = (InRule.Repository.XmlQualifiedNameInfo[])getValue();

        //        var value = new CodeArrayCreateExpression(typeof(InRule.Repository.XmlQualifiedNameInfo[]).ToCodeTypeReference(), typedPropValue.Select(i => RenderAssemblyDefInfo.Render(outerClass, i)).ToArray());
        //        target.SetProperty(propertyName, value);
        //    }

        //    else if (typeof(InRule.Repository.EntityDefsInfo).IsAssignableFrom(propertyType)
        //        || typeof(InRule.Repository.EndPoints.AssemblyDef.TypeConverterInfo).IsAssignableFrom(propertyType)
        //        || typeof(InRule.Repository.EndPoints.AssemblyDef.ClassInfo).IsAssignableFrom(propertyType)
        //        )
        //    {
        //        var typedPropValue = getValue();
        //        if (typedPropValue != null)
        //        {
        //            target.SetProperty(propertyName, RenderAssemblyDefInfo.Render(outerClass, typedPropValue));
        //        }
        //    }

        //    else
        //    {
        //        target.AddLiteralStatement(String.Format("Unhandled Type: '{3}' at {0}.SetProperty(\"{1}\", {2});", target.VariableName, propertyName, getValue(), propertyType.FullName));
        //    }

        //}

        //public bool TryRenderProperty<TType>(Func<CodeTypeDeclarationEx, TType, CodeExpression> render, CodeTypeDeclarationEx outerClass, string propertyName, Type propertyType, Func<object> getValue)
        //{
        //    var target = this;
        //    if (typeof(TType).IsAssignableFrom(propertyType))
        //    {
        //        var typedPropValue = (TType)getValue();
        //        if (typedPropValue != null)
        //        {
        //            target.SetProperty(propertyName, render(outerClass, typedPropValue));
        //        }
        //        return true;
        //    }
        //    return false;
        //}

        //public bool TryRenderCollectionProperty<TCollection, TType>(Func<CodeTypeDeclarationEx, TType, CodeExpression> render, CodeTypeDeclarationEx outerClass, string propertyName, Type propertyType, Func<object> getValue) where TCollection : IEnumerable
        //{
        //    var target = this;
        //    if (propertyType == typeof(TCollection))
        //    {
        //        var typedPropValue = (TCollection)getValue();
        //        if (typedPropValue != null)
        //        {
        //            foreach (TType def in typedPropValue)
        //            {
        //                target.GetPropertyReference(propertyName)
        //                    .AddInvokeMethodStatement("Add", render(outerClass, def));
        //            }
        //        }
        //        return true;
        //    }
        //    return false;
        //}
        //public bool TryRenderCollectionProperty<TCollection, TType>(Func<CodeTypeDeclarationEx, TType, CodeExpression> renderArg1, Func<CodeTypeDeclarationEx, TType, CodeExpression> renderArg2, CodeTypeDeclarationEx outerClass, string propertyName, Type propertyType, Func<object> getValue) where TCollection : IEnumerable
        //{
        //    var target = this;
        //    if (propertyType == typeof(TCollection))
        //    {
        //        var typedPropValue = (TCollection)getValue();
        //        if (typedPropValue != null)
        //        {
        //            foreach (TType def in typedPropValue)
        //            {
        //                target.GetPropertyReference(propertyName)
        //                    .AddInvokeMethodStatement("Add", renderArg1(outerClass, def), renderArg2(outerClass, def));
        //            }
        //        }
        //        return true;
        //    }
        //    return false;
        //}
        ////private static void SetPropertyForAttributes(CodeMethodVariableReferenceEx target, CodeTypeDeclarationEx outerClass, object propValue)
        ////{
        ////    var propCollections = (RuleRepositoryDefBase.DefMetadataCollectionCollection)propValue;
        ////    foreach (RuleRepositoryDefBase.DefMetadataCollectionCollection.CollectionItem propCollection in propCollections)
        //    {
        //        // DefaultAttributeGroupKey
        //        // ReservedInRuleAttributeGroupKey
        //        // ReservedInRuleTokenKey
        //        // ReservedInRuleVersionConversionKey
        //        // ReservedInRuleTemplateConversionKey

        //        CodeMethodVariableReferenceEx collection = null;


        //        if (propCollection.Key == RuleRepositoryDefBase.DefaultAttributeGroupKey)
        //        {
        //            collection = target.GetPropertyReference("Attributes");
        //        }
        //        else if (propCollection.Key == RuleRepositoryDefBase.ReservedInRuleAttributeGroupKey)
        //        {
        //            collection =
        //                target.GetPropertyReference("Attributes")
        //                    .GetIndexer(
        //                        typeof(RuleRepositoryDefBase).ToCodeTypeReference()
        //                            .ToCodeExpression(outerClass)
        //                            .GetCodeProperty("ReservedInRuleAttributeGroupKey"));
        //        }
        //        else if (propCollection.Key == RuleRepositoryDefBase.ReservedInRuleTokenKey)
        //        {
        //            continue; // Ignoring for now to make rendering much smaller
        //            collection =
        //                target.GetPropertyReference("Attributes")
        //                    .GetIndexer(
        //                        typeof(RuleRepositoryDefBase).ToCodeTypeReference()
        //                            .ToCodeExpression(outerClass)
        //                            .GetCodeProperty("ReservedInRuleTokenKey"));
        //        }
        //        else if (propCollection.Key == RuleRepositoryDefBase.ReservedInRuleVersionConversionKey)
        //        {
        //            collection =
        //                target.GetPropertyReference("Attributes")
        //                    .GetIndexer(
        //                        typeof(RuleRepositoryDefBase).ToCodeTypeReference()
        //                            .ToCodeExpression(outerClass)
        //                            .GetCodeProperty("ReservedInRuleVersionConversionKey"));
        //        }
        //        else if (propCollection.Key == RuleRepositoryDefBase.ReservedInRuleTemplateConversionKey)
        //        {
        //            collection =
        //                target.GetPropertyReference("Attributes")
        //                    .GetIndexer(
        //                        typeof(RuleRepositoryDefBase).ToCodeTypeReference()
        //                            .ToCodeExpression(outerClass)
        //                            .GetCodeProperty("ReservedInRuleTemplateConversionKey"));
        //        }
        //        else
        //        {
        //            var keyRef = typeof(AttributeGroupKey).CallCodeConstructor(propCollection.Key.Name.ToCodeExpression(outerClass),
        //                propCollection.Key.Guid.ToCodeExpression(outerClass));
        //            var keyRefVar = target.DeclareVariable("AttribKey_" + propCollection.Key.Name, typeof(AttributeGroupKey),
        //                keyRef);
        //            collection = target.GetPropertyReference("Attributes").GetIndexer(keyRefVar);
        //        }

        //        foreach (
        //            InRule.Repository.XmlSerializableStringDictionary.XmlSerializableStringDictionaryItem value in
        //                propCollection.Collection)
        //        {
        //            collection.SetIndexerValue(value.Key, value.Value);
        //        }
        //        int a = 1;
        //    }
        //}
    }
}