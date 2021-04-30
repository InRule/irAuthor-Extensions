using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using InRule.Repository;
using InRule.Repository.Classifications;
using InRule.Repository.Designer;
using InRule.Repository.EndPoints;
using InRule.Repository.RuleElements;
using InRule.Repository.UDFs;
using InRule.Repository.ValueLists;
using InRule.Repository.Vocabulary;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.Rendering
{
    public static partial class SdkCodeRenderingExtensions
    {
        public static string ToSdkCode(this object testValue)
        {
            Errors.Clear();
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            var ruleAppProvider = CodeDomProvider.CreateProvider("csharp");
            var ruleAppUnit = new CodeCompileUnit();
            var ruleAppNS = new CodeNamespace("InRuleLabs.SDKCodeGen");
            ruleAppNS.Types.Add(testValue.ToCodeClass());
            ruleAppUnit.Namespaces.Add(ruleAppNS);
            var options = new CodeGeneratorOptions();
            try
            {
                ruleAppProvider.GenerateCodeFromCompileUnit(ruleAppUnit, writer, options);
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.ToString());
            }
            
            var value = "//------------------------------------------------------------------------------";
            sb.Remove(0, sb.ToString().IndexOf(value, value.Length) + value.Length);

            foreach (var err in Errors)
            {
                sb.AppendLine(err);
            }

            var ret = sb.ToString();
            // Cleans up weird addition of "Referenced" and the end of lines assigning def.TemplateScope = InRule.Repository.Vocabulary.TemplateScope.Local;
            ret = ret.Replace(", Referenced;" + Environment.NewLine, ";" + Environment.NewLine);
            return ret;
        }

        public static string ToCSharpCode(this CodeExpression codeExpression)
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            var ruleAppProvider = CodeDomProvider.CreateProvider("csharp");
            var options = new CodeGeneratorOptions();
            try
            {
                ruleAppProvider.GenerateCodeFromExpression(codeExpression, writer, options);
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.ToString());
            }
            foreach (var err in Errors)
            {
                sb.AppendLine(err);
            }

            var ret = sb.ToString();
            return ret;
        }

        public static string ToCSharpCode(this CodeStatement codeStatement)
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            var ruleAppProvider = CodeDomProvider.CreateProvider("csharp");
            var options = new CodeGeneratorOptions();
            try
            {
                ruleAppProvider.GenerateCodeFromStatement(codeStatement, writer, options);
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.ToString());
            }
            foreach (var err in Errors)
            {
                sb.AppendLine(err);
            }

            var ret = sb.ToString();
            return ret;
        }
        public static CodeTypeDeclaration ToCodeClass(this object sourceDef)
        {

            var defClass = CodeTypeDeclarationEx.CreateRoot(sourceDef);

            var createMethod = defClass.AddCreateMethod(sourceDef, true);
            if (sourceDef is RuleRepositoryDefBase)
            {
                var def = createMethod.AddReturnVariable("def", sourceDef.GetType().CallCodeConstructor());
                AppendProperities(defClass, def, sourceDef);
                def.AddAsReturnStatement();
            }
            else
            {
                var def = createMethod.AddReturnVariable("def", sourceDef.GetType().CallCodeConstructor());
                createMethod.AddStatement(new CodeAssignStatement(def.Variable, sourceDef.ToCodeExpression2(defClass)));
                def.AddAsReturnStatement();
            }
            return defClass.InnerDeclaration;
        }

        private static HashSet<string> Errors = new HashSet<string>();

        
        #region Core
        public static CodeExpression NullCodeExpression = new CodePrimitiveExpression(null);

        public static CodeTypeReferenceEx ToCodeTypeReference(this Type type) { return new CodeTypeReferenceEx(type);}
        public static CodeExpression ToCodeExpression(this CodeTypeReference typeReference)
        {
            return new CodeTypeReferenceExpression(typeReference);
        }
        public static CodeObjectCreateExpression CallCodeConstructor(this Type type, params CodeExpression[] parameters)
        {
            return new CodeObjectCreateExpression(type, parameters);
        }
        public static CodeExpression GetCodeProperty(this CodeExpression expression, string propName)
        {
            return new CodePropertyReferenceExpression() { PropertyName = propName, TargetObject = expression };
        }
#region Def Factory Methods
        public static CodeDefPropertyDescriptor ToParameter<T>(this string propertyName)
        {
            return new CodeDefPropertyDescriptor(propertyName, typeof(T));
        }
        /// <summary>
        /// Appends arguments to the method signiture and the property assignments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="outerClass"></param>
        /// <param name="factoryInvoke"></param>
        /// <param name="requiredArgs"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public static void AppendFactoryDefPropertySetter<T>(this CodeTypeDeclarationEx outerClass, CodeMethodInvokeExpression factoryInvoke, List<CodeDefPropertyDescriptor> requiredArgs, string propertyName, T propertyValue)
        {
            factoryInvoke.Parameters.Add(propertyValue.ToCodeExpression2(outerClass));
            requiredArgs.Add(new CodeDefPropertyDescriptor(propertyName, typeof(T)));
        }
        public static void EnsureSharedFactoryMethod(this CodeTypeDeclarationEx fromOuterClass, CodeTypeReferenceEx returnType, string methodName, params CodeDefPropertyDescriptor[] parameters)
        {

            var defFactory = fromOuterClass.GetDefFactory();

            if (!defFactory.ContainsSharedFactoryMethod(methodName, parameters))
            {
                CodeMethodVariableReferenceEx returnVariable;
                var method = defFactory.AddFactoryMethod(returnType, methodName, out returnVariable);
                method.DefPropertyAssignments = parameters;
                foreach (var arg in parameters)
                {
                    AppendAssignPropertyFromParameter(method, returnVariable, arg);
                }
                returnVariable.AddAsReturnStatement();
            }
        }
        #endregion
        public static bool TryCreateCodeExpressionFrom<T>(this object sourceDef, Func<T, CodeExpression> renderDelgate, out CodeExpression render)
        {
            // return sourceDef.TryRenderToCodeExpression<ClassificationDef>((obj) => obj.ToCodeExpression(outerClass), out render);

            if (sourceDef.IsInstanceOf<T>())
            {
                render = renderDelgate((T)sourceDef);
                return true;
            }
            render = null;
            return false;
        }
        public static bool TryToCodeExpressionDerivedFrom<T>(this object sourceDef, Func<T, CodeExpression> renderDelgate, out CodeExpression render)
        {

            if (sourceDef != null)
            {
                if (typeof(T).IsAssignableFrom(sourceDef.GetType()))
                {
                    render = renderDelgate((T)sourceDef);
                    return true;
                }
            }

            render = null;
            return false;

        }


        public static void AddSetProperty(this CodeMemberMethodEx method, CodeExpression variable, string propertyName, CodeExpression value)
        {
            if (propertyName == "TemplateScope")
            {
                int a = 1;
            }
            var propRef = new CodePropertyReferenceExpression();
            propRef.TargetObject = variable;
            propRef.PropertyName = propertyName;

            var setProp = new CodeAssignStatement(propRef, value);
            method.Statements.Add(setProp);

            var code = setProp.ToCSharpCode();
        }

        #endregion core

        public static CodeExpression ToCodeExpression2(this object source, CodeTypeDeclarationEx outerClass)
        {
            CodeExpression codeExpression;
            if (source == null) {return NullCodeExpression;}

            if (source.TryCreateCodeExpressionFrom <Guid>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<Enum>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<DateTime>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<string>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<decimal>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom <DBNull>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom <bool>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<ulong>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<long>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<ushort>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<short>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<uint>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<int>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<double>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<float>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<char>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<byte>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<sbyte>((v) => v.ToCodeExpression(), out codeExpression)) return codeExpression;

            if (source.TryCreateCodeExpressionFrom<DataSet>((v) => v.ToCodeExpression(outerClass), out codeExpression)) return codeExpression;
            if (source.TryCreateCodeExpressionFrom<DataTable>((v) => v.ToCodeExpression(outerClass), out codeExpression)) return codeExpression;

            if (TryRenderAsDefSpecific(source, outerClass, out codeExpression)) return codeExpression;


            if (TryRenderNamedDefAsClass<FieldDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderNamedDefAsClass<RuleApplicationDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderNamedDefAsClass<InRule.Repository.Decisions.DecisionDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderNamedDefAsClass<RuleSetDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderNamedDefAsClass<EntityDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderNamedDefAsClass<InRule.Repository.DecisionTables.DecisionTableDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderNamedDefAsClass<UdfLibraryDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderNamedDefAsClass<VocabularyDef>(outerClass, source, out codeExpression)) return codeExpression;
            
            if (TryRenderAsAuthoringSettings(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<AssemblyDef.InfoBase>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.DecisionTables.ConditionDimensionDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.DecisionTables.ConditionValueDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.DecisionTables.ActionValueDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.DecisionTables.DecisionDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.DecisionTables.ActionDimensionDef>(outerClass, source, out codeExpression)) return codeExpression;


            if (TryRenderAsClassType<RestrictionInfo>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<AssemblyDef.TypeConverterInfo>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<EntityDefsInfo>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<SecurityPermission>(outerClass, source, out codeExpression)) return codeExpression;
            

            if (TryRenderAsClassType<InRule.Repository.RuleFlow.RuleFlowDesignerLayout>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.RuleFlow.RuleFlowDesignerItemLayout>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.XmlQualifiedNameInfo>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Common.Xml.Schema.Xsd.EmbeddedXmlSchema>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.WebServiceDef.OperationDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.WebServiceDef.PortDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.WebServiceDef.ServicesDef>(outerClass, source, out codeExpression)) return codeExpression;
            if (TryRenderAsClassType<InRule.Repository.EndPoints.RuleWriteInfo>(outerClass, source, out codeExpression)) return codeExpression;
            if (source.TryRenderAsGenericDef(outerClass, out codeExpression)) return codeExpression;

            if (source.TryRenderAsUnknownObject(outerClass, out codeExpression)) return codeExpression;

            

            var type = source.GetType();
            var message = "Unhandled Render Type " + type.FullName;
            
            if (Errors.Add(message))
            {
                Debug.WriteLine(message);
            }
            return ("Unhandled Render Type " + type.FullName).ToCodeExpression();
        }

        private static bool TryRenderAsDefSpecific(object source, CodeTypeDeclarationEx outerClass, out CodeExpression codeExpression)
        {
            
            if (source.TryCreateCodeExpressionFrom<ValueListItemDef>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<SendMailActionAttachmentDef>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<string[]>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<InRule.Repository.DecisionTables.ConditionNodeDef>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<InRule.Repository.DecisionTables.ActionNodeDef>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<NameSortOrderDef>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<ClassificationDef>((v) => v.ToCodeExpression(outerClass), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<ExecuteSqlQueryActionParameterValueDef>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<NameExpressionPairDef>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<ExecuteMethodActionParamDef>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<TypeMapping>((v) => v.ToCodeExpression(outerClass), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<CalcDef>((v) => v.ToCodeExpression(outerClass), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<RuleSetParameterDef>((v) => v.ToCodeExpression(outerClass), out codeExpression)){return true;}
            if (source.TryCreateCodeExpressionFrom<ExecuteRulesetParameterDef>((v) => v.ToCodeExpression(), out codeExpression)){return true;}
            if (source.TryCreateCodeExpressionFrom<DataTypeValueDef>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }
            if (source.TryCreateCodeExpressionFrom<EntityValueDef>((v) => v.ToCodeExpression(), out codeExpression)) { return true; }

            
            return false;
        }

        public static bool TryRenderAsGenericDef(this object sourceDef, CodeTypeDeclarationEx outerClass, out CodeExpression render)
        {

            if (typeof(RuleRepositoryDefBase).IsAssignableFrom(sourceDef.GetType()))
            {
                render = sourceDef.RenderGenericDefCreateMethod(outerClass);
                return true;
            }

            render = null;
            return false;
        }
        public static bool TryRenderAsUnknownObject(this object sourceDef, CodeTypeDeclarationEx outerClass, out CodeExpression render)
        {

           render = sourceDef.RenderGenericDefCreateMethod(outerClass);
           return true;
        }

        public static CodeExpression RenderGenericDefCreateMethod(this object sourceDef, CodeTypeDeclarationEx outerClass)
        {
            var defClass = outerClass;
            var createMethod = outerClass.AddCreateMethod(sourceDef);
            var render = new CodeMethodInvokeExpression(null, createMethod.Name);
            var factoryMethod = createMethod.AddReturnVariable("def", sourceDef.GetType().CallCodeConstructor());
            AppendProperities(defClass, factoryMethod, sourceDef);
            factoryMethod.AddAsReturnStatement();
            return render;
        }

        private static bool TryRenderNamedDefAsClass<T>(CodeTypeDeclarationEx outerClass, object sourceDef, out CodeExpression render) where T : RuleRepositoryDefBase
        {
            if (typeof(T).IsAssignableFrom(sourceDef.GetType()))
            {
                var typedValue = (T)sourceDef;

                var defClass = outerClass.AddChildClass(sourceDef, typedValue.Name);
                var createMethod = defClass.AddCreateMethod(sourceDef, true);
                var retMethod = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(defClass.Name),
                    createMethod.Name);
                var returnVariable = createMethod.AddReturnVariable("def", sourceDef.GetType().CallCodeConstructor());
                AppendProperities(defClass, returnVariable, sourceDef);
                returnVariable.AddAsReturnStatement();
                render = retMethod;
                return true;
            }
            render = null;
            return false;
        }

        #region Type Helpers 
        // Type Helpers
        public static ConstructorInfo GetPublicDefaultConstructor(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null || !constructor.IsPublic)
            {
                var ctors =
                    from ctor in type.GetConstructors().Where(cons=>cons.IsPublic)
                    let prms = ctor.GetParameters()
                    where prms.All(p => p.IsOptional)
                    orderby prms.Length
                    select ctor;
                constructor = ctors.FirstOrDefault();
            }
            return constructor;
        }

        public static bool HasDefaultConstructor(this Type type)
        {
            return type.GetPublicDefaultConstructor() != null;
        }
        public static bool IsInstanceOf<T>(this object instance) { return instance is T;}
        #endregion
        #region // Primitives

        #endregion


        //Defs
        #region Defs
        public static void AppendAssignPropertyFromParameter(CodeMemberMethodEx method, CodeMethodVariableReferenceEx returnValue, CodeDefPropertyDescriptor parameter)
        {
            var parameterName = parameter.Name.Substring(0, 1).ToLower() + parameter.Name.Substring(1);
            method.Parameters.Add(new CodeParameterDeclarationExpression()
            {
                Name = parameterName,
                Type = parameter.Type.ToCodeTypeReference()
            });
            returnValue.SetProperty(parameter.Name, new CodeVariableReferenceExpression() { VariableName = parameterName });
        }
        public static void AppendAssignPropertyFromParameter(CodeMemberMethodEx method, CodeMethodVariableReferenceEx returnValue, string defPropName, Type dataType)
        {
            AppendAssignPropertyFromParameter(method, returnValue, new CodeDefPropertyDescriptor(defPropName, dataType));
        }


        #endregion
        #region Tree Walkers
        private static bool HasAttribute(MemberInfo propInfo, Type attributeType)
        {
            var attribs = propInfo.GetCustomAttributes();
            foreach (var attrib in attribs)
            {
                if (attributeType.IsAssignableFrom(attrib.GetType()))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool SkipBoringProperty(string name)
        {
            return false;
            switch (name)
            {
                case "RuntimeEngine":
                case "FeatureVersion":
                case "CompatibilityVersion":
                case "IndentUnboundCollectionXml":
                case "AllowRuleInactivation":
                case "UseRuleVersions":
                case "UseVersionCreationDates":
                case "HasContextVersionSettings":
                case "RunawayCycleCount":
                case "Timeout":
                case "RuntimeErrorHandlingPolicy":
                case "LastValidateContentCode":
                case "LastValidateDateTimeUtc":
                case "IsolatedTestDomain":
                case "SchemaGuid":
                case "SchemaRevision":
                case "SchemaPublicRevision":
                case "Id":
                case "RepositoryAssemblyFileVersion":
                case "UpgraderMessageList":
                case "PublicRevision":
                case "Revision":
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ShouldWriteProperty(PropertyInfo propInfo)
        {

            if (propInfo.DeclaringType == typeof( RuleApplicationDef) && propInfo.Name == "Decisions")
            {
                return true;
            }

            if (SkipBoringProperty(propInfo.Name)) return false;
            if (HasAttribute(propInfo, typeof(XmlIgnoreAttribute))) return false;
            // There are some collections that do not have setters
            if (IsDerivedFromObservableCollection(propInfo.PropertyType)) return true;
            if (IsDerivedFromRuleDefCollection(propInfo.PropertyType)) return true;
            if (!propInfo.CanWrite) return false;
            if (!propInfo.SetMethod.IsPublic) return false;
            if (propInfo.SetMethod.IsStatic) return false;
            

            return true;
        }

        private static bool ShouldWriteField(FieldInfo propInfo)
        {
            if (SkipBoringProperty(propInfo.Name)) return false;
            if (propInfo.IsLiteral) return false;
            if (!propInfo.IsPublic) return false;
            if (propInfo.IsStatic) return false;
            if (HasAttribute(propInfo, typeof(XmlIgnoreAttribute))) return false;

            return true;
        }

        private static void AppendProperities(CodeTypeDeclarationEx outerClass, CodeMethodVariableReferenceEx target, object sourceDef)
        {
            foreach (var propInfo in sourceDef.GetType().GetProperties().Where(p => ShouldWriteProperty(p)))
            {
                HandleProperty(outerClass, target, sourceDef, propInfo);
            }
            foreach (var propInfo in sourceDef.GetType().GetFields().Where(p => ShouldWriteField(p)))
            {
                HandleField(outerClass, target, sourceDef, propInfo);
            }
        }

        private static void HandleField(CodeTypeDeclarationEx outerClass, CodeMethodVariableReferenceEx target, object sourceDef, FieldInfo fieldInfo)
        {
            if (IsSpecifiedFalse(sourceDef, fieldInfo.Name)) return;

            var value = fieldInfo.GetValue(sourceDef);
            if (TryRenderAsCollection(outerClass, target, fieldInfo.Name, value)) return;
            SetProperty(target, fieldInfo.Name, value.ToCodeExpression2(outerClass));

        }

        private static void SetProperty(CodeMethodVariableReferenceEx target, string name, CodeExpression value)
        {

            target.Method.AddSetProperty(target.Variable, name, value);
        }

        private static void HandleProperty(CodeTypeDeclarationEx outerClass, CodeMethodVariableReferenceEx target, object sourceDef, PropertyInfo propInfo)
        {
  
            if (IsSpecifiedFalse(sourceDef, propInfo.Name)) return;

            var value = propInfo.GetValue(sourceDef);

            if (TryRenderAsCollection(outerClass, target, propInfo.Name, value)) return;
            SetProperty(target, propInfo.Name, value.ToCodeExpression2(outerClass));
        }

        private static bool IsSpecifiedFalse(object sourceDef, string name)
        {
            var specifiedProp = sourceDef.GetType().GetProperty(name + "Specified");
            if (specifiedProp != null)
            {
                var specified = specifiedProp.GetValue(sourceDef);
                if (specified is bool && ((bool)specified) == false)
                {
                    return true;
                }
            }

            var specifiedField = sourceDef.GetType().GetField(name + "Specified");
            if (specifiedField != null)
            {
                var specified = specifiedField.GetValue(sourceDef);
                if (specified is bool && ((bool)specified) == false)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion



        private static bool TryRenderAsCollection(CodeTypeDeclarationEx outerClass, CodeMethodVariableReferenceEx target, string propertyName, object sourceDef)
        {

            if (sourceDef == null) return false;
            if (TryRenderAsRuleRepositoryDefCollection<RuleRepositoryDefCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderObservableCollectionProperty(outerClass, target, propertyName, sourceDef)) return true;
            
            if (TryRenderCollectionProperty<InRule.Repository.DecisionTables.ConditionValueDefCollection>(outerClass, target, propertyName, sourceDef)) return true;

            if (TryRenderAsRuleRepositoryDefCollection<SecurityPermissionCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderAsStringCollection(target, propertyName, sourceDef)) return true;
            
            if (TryRenderAsDefMetaDataCollection(outerClass, target, propertyName, sourceDef)) return true;

            if (TryRenderCollectionProperty<InRule.Repository.Utilities.ExpressionDictionary, KeyValuePair<string, string>>((outer, def) => def.Key.ToCodeExpression(), (outer, def) => def.Value.ToCodeExpression(), outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.EndPoints.AssemblyDef.ClassInfoCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.EntityDefInfoCollection>(outerClass, target, propertyName, sourceDef)) return true;

            if (TryRenderCollectionProperty<InRule.Repository.DecisionTables.ConditionDimensionDefCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.DecisionTables.ActionDimensionDefCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.DecisionTables.DecisionDefCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.DecisionTables.ConditionValueDefCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.DecisionTables.ActionValueDefCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.DecisionTables.ConditionNodeDefCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.DecisionTables.ActionNodeDefCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.WebServiceDef.OperationDefCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.WebServiceDef.PortDefCollection>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<InRule.Repository.WebServiceDef.ServicesDefCollection>(outerClass, target, propertyName, sourceDef)) return true;

            if (TryRenderXmlSerializableStringDictionaryCollectionPropertyWithIndexer(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderAsSetArrayValue<AssemblyDef.ClassMethodInfo[]>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderAsSetArrayValue<AssemblyDef.ClassPropertyInfo[]>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderAsSetArrayValue<AssemblyDef.ClassFieldInfo[]>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderAsSetArrayValue<InRule.Repository.EndPoints.RuleWriteInfo[]>(outerClass, target, propertyName, sourceDef)) return true;



            if (TryRenderAsSetArrayValue<RestrictionInfo[]>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderAsSetArrayValue<AssemblyDef.ClassParameterInfo[]>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderAsSetArrayValue<InRule.Repository.XmlQualifiedNameInfo[]>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderAsSetArrayValue<InRule.Common.Xml.Schema.Xsd.EmbeddedXmlSchema[]>(outerClass, target, propertyName, sourceDef)) return true;
            if (TryRenderCollectionProperty<List<DesignerItemLayout>>(outerClass, target, propertyName, sourceDef)) return true;
            return false;
        }

        private static bool TryRenderAsSetArrayValue<TArray>(CodeTypeDeclarationEx outerClass, CodeMethodVariableReferenceEx target, string propertyName, object sourceDef) where TArray : IEnumerable
        {
            if (!typeof(TArray).IsAssignableFrom(sourceDef.GetType())) return false;

            TArray typedValue;
            try
            {
                typedValue = (TArray)sourceDef;
            }
            catch (Exception)
            {
                return false;
            }

            if (typedValue != null)
            {
                List<CodeExpression> vals = new List<CodeExpression>();
                foreach (var val in typedValue)
                {
                    vals.Add(val.ToCodeExpression2(outerClass));
                }
                var array = new CodeArrayCreateExpression(typeof(TArray).ToCodeTypeReference(), vals.ToArray());
                SetProperty(target, propertyName, array);
                return true;
            }
            return false;
        }

        private static bool TryRenderAsClassType<T>(CodeTypeDeclarationEx outerClass, object sourceDef, out CodeExpression render)
        {
            if (typeof(T).IsAssignableFrom(sourceDef.GetType()))
            {
                string name = null;
                var defClass = outerClass;
                var createMethod = outerClass.AddCreateMethod(sourceDef);
                render = new CodeMethodInvokeExpression(null, createMethod.Name);

                var renderedCalcDef = createMethod.AddReturnVariable("def", sourceDef.GetType().CallCodeConstructor());
                AppendProperities(defClass, renderedCalcDef, sourceDef);
                renderedCalcDef.AddAsReturnStatement();
                return true;
            }

            render = null;
            return false;
        }

        private static bool TryRenderAsAuthoringSettings(CodeTypeDeclarationEx outerClass, object sourceDef,
            out CodeExpression render)
        {
            var typedValue = sourceDef as RuleApplicationAuthoringSettings;
            if (typedValue != null)
            {
                var createMethod = outerClass.AddCreateMethod(sourceDef);
                render = new CodeMethodInvokeExpression(null, createMethod.Name);
                var renderedCalcDef = createMethod.AddReturnVariable("def", sourceDef.GetType().CallCodeConstructor());
                AppendProperities(outerClass, renderedCalcDef, sourceDef);
                renderedCalcDef.AddAsReturnStatement();
                return true;
            }

            render = null;
            return false;
        }

        public static bool TryRenderAsDefMetaDataCollection(CodeTypeDeclarationEx outerClass,
            CodeMethodVariableReferenceEx target, string propertyName, object sourceDef)
        {
            var typedValue = sourceDef as RuleRepositoryDefBase.DefMetadataCollectionCollection;
            if (typedValue != null)
            {
                SetPropertyForAttributes(target, sourceDef);
                return true;
            }
            return false;
        }

        public static bool RenderHintAttributes = true;
        private static void SetPropertyForAttributes(CodeMethodVariableReferenceEx target, object propValue)
        {
            var propCollections = (RuleRepositoryDefBase.DefMetadataCollectionCollection)propValue;
            foreach (
                RuleRepositoryDefBase.DefMetadataCollectionCollection.CollectionItem propCollection in propCollections)
            {
                // DefaultAttributeGroupKey
                // ReservedInRuleAttributeGroupKey
                // ReservedInRuleTokenKey
                // ReservedInRuleVersionConversionKey
                // ReservedInRuleTemplateConversionKey

                CodeMethodVariableReferenceEx collection = null;


                if (propCollection.Key == RuleRepositoryDefBase.DefaultAttributeGroupKey)
                {
                    collection = target.GetPropertyReference("Attributes");
                }
                else if (propCollection.Key == RuleRepositoryDefBase.ReservedInRuleAttributeGroupKey)
                {
                    collection =
                        target.GetPropertyReference("Attributes")
                            .GetIndexer(
                                typeof(RuleRepositoryDefBase).ToCodeTypeReference()
                                    .ToCodeExpression()
                                    .GetCodeProperty("ReservedInRuleAttributeGroupKey"));
                }
                else if (propCollection.Key == RuleRepositoryDefBase.ReservedInRuleTokenKey)
                {
                    if (RenderHintAttributes)
                    {
                        collection =
                            target.GetPropertyReference("Attributes")
                                .GetIndexer(
                                    typeof(RuleRepositoryDefBase).ToCodeTypeReference()
                                        .ToCodeExpression()
                                        .GetCodeProperty("ReservedInRuleTokenKey"));
                    }
                }
                else if (propCollection.Key == RuleRepositoryDefBase.ReservedInRuleVersionConversionKey)
                {
                    collection =
                        target.GetPropertyReference("Attributes")
                            .GetIndexer(
                                typeof(RuleRepositoryDefBase).ToCodeTypeReference()
                                    .ToCodeExpression()
                                    .GetCodeProperty("ReservedInRuleVersionConversionKey"));
                }
                else if (propCollection.Key == RuleRepositoryDefBase.ReservedInRuleTemplateConversionKey)
                {
                    collection =
                        target.GetPropertyReference("Attributes")
                            .GetIndexer(
                                typeof(RuleRepositoryDefBase).ToCodeTypeReference()
                                    .ToCodeExpression()
                                    .GetCodeProperty("ReservedInRuleTemplateConversionKey"));
                }
                else
                {
                    var keyRef =
                        typeof(AttributeGroupKey).CallCodeConstructor(
                            propCollection.Key.Name.ToCodeExpression(), propCollection.Key.Guid.ToCodeExpression());
                    var keyRefVar = target.DeclareVariable("AttribKey_" + propCollection.Key.Name,
                        typeof(AttributeGroupKey),
                        keyRef);
                    collection = target.GetPropertyReference("Attributes").GetIndexer(keyRefVar);
                }

                foreach (
                    InRule.Repository.XmlSerializableStringDictionary.XmlSerializableStringDictionaryItem value in
                        propCollection.Collection)
                {
                    collection.SetIndexerValue(value.Key.ToCodeExpression(), value.Value.ToCodeExpression());
                }

            }
        }
        
        public static bool TryRenderXmlSerializableStringDictionaryCollectionPropertyWithIndexer(CodeTypeDeclarationEx outerClass, CodeMethodVariableReferenceEx target, string propertyName, object value)
        {

            if (value.GetType() == typeof(XmlSerializableStringDictionary))
            {
                var typedPropValue = (XmlSerializableStringDictionary)value;
                if (typedPropValue != null)
                {
                    var collection = target.GetPropertyReference(propertyName);
                    foreach (InRule.Repository.XmlSerializableStringDictionary.XmlSerializableStringDictionaryItem def in typedPropValue)
                    {
                        collection.SetIndexerValue(def.Key.ToCodeExpression(), def.Value.ToCodeExpression());
                    }
                }
                return true;
            }
            return false;
        }
        public static bool TryRenderCollectionProperty<TCollection>(CodeTypeDeclarationEx outerClass, CodeMethodVariableReferenceEx target, string propertyName, object value) where TCollection : IEnumerable
        {

            if (value.GetType() == typeof(TCollection))
            {
                var typedPropValue = (TCollection)value;
                if (typedPropValue != null)
                {
                    foreach (object def in typedPropValue)
                    {
                        target.GetPropertyReference(propertyName)
                            .AddInvokeMethodStatement("Add", def.ToCodeExpression2(outerClass));
                    }
                }
                return true;
            }
            return false;
        }

        public static List<Type> GetInheritanceStack(Type type)
        {
            var ret = new List<Type>();
            while (type != null)
            {
                ret.Add(type);
                type = type.BaseType;
            }

            return ret;
        }
        
        public static bool IsDerivedFromRuleDefCollection(Type type)
        {
            var stack = GetInheritanceStack(type);
            return stack.Any(r => r.Name.StartsWith("RuleRepositoryDefCollection"));
        }
        public static bool IsDerivedFromObservableCollection(Type type)
        {
            var stack = GetInheritanceStack(type);
            return stack.Any(r => r.Name.StartsWith("ObservableCollection"));
        }
        public static bool TryRenderObservableCollectionProperty(CodeTypeDeclarationEx outerClass, CodeMethodVariableReferenceEx target, string propertyName, object value)
        {
            if (value == null) return false;

            if (IsDerivedFromObservableCollection(value.GetType()))
            {
                   var typedPropValue = (IEnumerable)value;
                if (typedPropValue != null)
                {
                    foreach (object def in typedPropValue)
                    {
                        target.GetPropertyReference(propertyName)
                            .AddInvokeMethodStatement("Add", def.ToCodeExpression2(outerClass));
                    }
                }
                return true;
            }
            return false;
        }
        public static bool TryRenderCollectionProperty<TCollection, TType>(Func<CodeTypeDeclarationEx, TType, CodeExpression> renderArg1, Func<CodeTypeDeclarationEx, TType, CodeExpression> renderArg2, CodeTypeDeclarationEx outerClass, CodeMethodVariableReferenceEx target, string propertyName, object value) where TCollection : IEnumerable
        {
            if (value.GetType() == typeof(TCollection))
            {
                var typedPropValue = (TCollection)value;
                if (typedPropValue != null)
                {
                    foreach (TType def in typedPropValue)
                    {
                        target.GetPropertyReference(propertyName)
                            .AddInvokeMethodStatement("Add", renderArg1(outerClass, def), renderArg2(outerClass, def));
                    }
                }
                return true;
            }
            return false;
        }

        private static bool TryRenderAsRuleRepositoryDefCollection<T>(CodeTypeDeclarationEx outerClass, CodeMethodVariableReferenceEx target, string propertyName, object sourceDef) where T : IEnumerable
        {
            if (typeof(T).IsAssignableFrom(sourceDef.GetType()))
            {
                var typedValue = (T)sourceDef;
                if (typedValue != null)
                {
                    foreach (var def in typedValue)
                    {
                        target.GetPropertyReference(propertyName)
                            .AddInvokeMethodStatement("Add", def.ToCodeExpression2(outerClass));
                    }
                    return true;
                }
            }
            return false;
        }
        
        private static bool TryRenderAsStringCollection(CodeMethodVariableReferenceEx target, string propertyName, object sourceDef)
        {
            var typedValue = sourceDef as StringCollection;
            if (typedValue != null)
            {

                var collection = target.GetPropertyReference(propertyName);
                List<CodeExpression> vals = new List<CodeExpression>();
                foreach (var val in typedValue)
                {
                    vals.Add(val.ToCodeExpression());
                }
                var array = new CodeArrayCreateExpression(typeof(String[]).ToCodeTypeReference(), vals.ToArray());
                collection.AddInvokeMethodStatement("AddRange", array);

                return true;
            }
            return false;
        }
    }
}
    