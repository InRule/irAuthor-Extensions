using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InRule.Authoring.BusinessLanguage;
using InRule.Authoring.BusinessLanguage.Runtime;
using InRule.Authoring.BusinessLanguage.Templates;
using InRule.Repository;
using InRule.Repository.Infos;
using InRule.Repository.RuleElements;
using InRule.Repository.Vocabulary;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Extensions
{
    public static class DefUsageExtensions
    {
        public static RuleRepositoryDefBase GetDef(this Guid guid, RuleApplicationDef ruleAppDef)
        {
            return ruleAppDef.LookupItem(guid);
        }
        public static RuleSetDef GetParentRulesetDef(this RuleRepositoryDefBase ruleDef)
        {
            if (ruleDef is RuleSetDef) return (RuleSetDef) ruleDef;
            if (ruleDef.Parent == null)
            {
                return null;
            }
            return GetParentRulesetDef(ruleDef.Parent);
        }
        public static List<RuleRepositoryDefBase> GetUnusedFields(this RuleApplicationDef ruleAppDef)
        {
            var fields = ruleAppDef.GetAllFields().ToList();
            foreach (var field in ruleAppDef.GetConsumedFields())
            {
                fields.Remove(field.TargetDef);
            }
            foreach (var field in ruleAppDef.GetUpdatedFields())
            {
                fields.Remove(field.TargetDef);
            }
            
            return fields;
        }
        public static Stack<RuleRepositoryDefBase> GetAllFields(this RuleApplicationDef ruleAppDef)
        {
            var ret = new Stack<RuleRepositoryDefBase>();
            foreach (var entityDef in ruleAppDef.Entities.OfType<EntityDef>())
            {
                foreach (var fieldDef in entityDef.Fields.OfType<FieldDef>())
                {
                    ret.Push(fieldDef);
                }
            }
            return ret;
        }
        public static List<UpdatedDefInfo> GetUpdatedFields(this InRule.Repository.RuleApplicationDef ruleAppDef)
        {
            var updatedList = new List<UpdatedDefInfo>();
            var network = DefUsageNetwork.Create(ruleAppDef);
            foreach (var entityDef in ruleAppDef.Entities.OfType<EntityDef>())
            {
                foreach (var fieldDef in entityDef.Fields.OfType<FieldDef>())
                {
                    if (fieldDef.IsCalculated)
                    {
                        if (!updatedList.Any(x => x.TargetDef == fieldDef))
                        {
                            var defInfo = new UpdatedDefInfo();
                            defInfo.TargetDef = fieldDef;
                            defInfo.IsCalculation = true;
                            updatedList.Add(defInfo);
                        }
                    }
                    else
                    {
                        var usages = network.GetDefUsages(fieldDef.Guid, false);
                        foreach (var usage in usages)
                        {
                            if (usage.UsageType == DefUsageType.UpdatedBy)
                            {
                                UpdatedDefInfo defInfo =
                                    updatedList.FirstOrDefault(x => x.TargetDef == fieldDef);
                                if (defInfo == null)
                                {
                                    defInfo = new UpdatedDefInfo();
                                    defInfo.TargetDef = fieldDef;
                                    updatedList.Add(defInfo);
                                }
                                var relatedDef = usage.Element.GetDef(ruleAppDef);
                                defInfo.UpdatedBy.Add(relatedDef);
                            }
                        }
                    }
                }
            }
            return updatedList;
        }

        public static List<ConsumedDefInfo> GetConsumedFields(this InRule.Repository.RuleApplicationDef ruleAppDef)
        {
            var consumedList = new List<ConsumedDefInfo>();
            var network = DefUsageNetwork.Create(ruleAppDef);
            foreach (var entityDef in ruleAppDef.Entities.OfType<EntityDef>())
            {
                foreach (var fieldDef in entityDef.Fields.OfType<FieldDef>())
                {
                    var usages = network.GetDefUsages(fieldDef.Guid, false);
                    foreach (var usage in usages)
                    {
                        if (usage.UsageType == DefUsageType.ConsumedBy)
                        {
                            var defInfo = consumedList.FirstOrDefault(x => x.TargetDef == fieldDef);
                            if (defInfo == null)
                            {
                                defInfo = new ConsumedDefInfo();
                                defInfo.TargetDef = fieldDef;
                                consumedList.Add(defInfo);
                            }
                            var relatedDef = usage.Element.GetDef(ruleAppDef);
                            defInfo.ConsumedBy.Add(relatedDef);
                        }
                    }
                }
            }
            return consumedList;
        }


        public static string GetCommaSeperatedNameList(this List<RuleRepositoryDefBase> items)
        {
            var consumedBy = string.Join(" , ", items.Select(x => x.Name).ToArray());
            return consumedBy;
        }

        public static string GetFieldUsageSummary(this InRule.Repository.RuleApplicationDef ruleAppDef)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"----- Field Usage Summary -----");

            sb.AppendLine();
            sb.AppendLine(BuildFieldList($"UNUSED FIELDS", "(Unused)", ruleAppDef.GetUnusedFields()));

            sb.AppendLine();
            sb.AppendLine(BuildFieldList($"CONSUMED FIELDS", "(Consumed)", ruleAppDef.GetConsumedFields().Select(t => t.TargetDef).ToList()));

            sb.AppendLine();
            sb.AppendLine(BuildFieldList($"UPDATED FIELDS", "(Updated)", ruleAppDef.GetUpdatedFields().Select(t => t.TargetDef).ToList()));

            return sb.ToString();
        }

        private static string BuildFieldList(string title, string prefix, List<RuleRepositoryDefBase> elementList)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);
            foreach (var fld in elementList.OrderBy(t => t.AuthoringElementPath))
            {
                sb.AppendLine($"{prefix} {fld.AuthoringElementPath}");
            }
            return sb.ToString();
        }

        public static string GetDefUsageSummary(this InRule.Repository.RuleApplicationDef ruleAppDef)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"----- Field Usage Summary -----");

            sb.AppendLine();
            sb.AppendLine(BuildFieldList($"UNUSED FIELDS", "(Unused)", ruleAppDef.GetUnusedFields()));

            sb.AppendLine();
            sb.AppendLine(BuildFieldList($"CONSUMED FIELDS", "(Consumed)", ruleAppDef.GetConsumedFields().Select(t => t.TargetDef).ToList()));

            sb.AppendLine();
            sb.AppendLine(BuildFieldList($"UPDATED FIELDS", "(Updated)", ruleAppDef.GetUpdatedFields().Select(t => t.TargetDef).ToList()));

            return sb.ToString();
        }

      
        public static string DescribeAllRuntimeTemplates(this IEnumerable<RuntimeTemplate> runtimeTemplates)
        {
            var results = runtimeTemplates.Select(r => r.ToString()).OrderBy(r => r);
            return string.Join(Environment.NewLine, results);
        }

        public static string DescribeRuntimeTemplates(this IEnumerable<RuntimeTemplate> runtimeTemplates, bool sortByCount, string prefix = "")
        {
            var matches = runtimeTemplates.Where(p => p.Template.Name.ToLower().Contains("Copy")).ToList();

            var results = runtimeTemplates.GroupBy(
                p => p.Template.Name,
                (key, g) => new { Name = key, Count = g.Count() });

            results = results.OrderBy(r => r.Name);
            if (sortByCount)
            {
                results = results.OrderByDescending(r => r.Count);
            }

            var runtimeTemplatesPerTemplate = string.Join(Environment.NewLine, results.Select(t => prefix + t.Count.ToString().PadRight(5) + t.Name));
            return runtimeTemplatesPerTemplate;
        }
        public static List<RuleRepositoryDefBase> GetAllDefs(this RuleRepositoryDefBase parentDef)
        {
            var items = new HashSet<RuleRepositoryDefBase>();
            parentDef.CollectAllDefs(items);
            return items.ToList();
        }

        private static void CollectAllDefs(this RuleRepositoryDefBase parentDef, HashSet<RuleRepositoryDefBase> items)
        {
            if (items.Add(parentDef))
            {
                foreach (RuleRepositoryDefBase def in parentDef.GetAllRootItems())
                {
                    def.CollectAllDefs(items);
                }

                IContainsVocabulary containsVocabulary = parentDef as IContainsVocabulary;
                VocabularyDef vocab = containsVocabulary?.Vocabulary;
                if (vocab != null)
                {
                    items.Add(vocab);
                }
                
                var languageRuleDef = parentDef as LanguageRuleDef;
                if (languageRuleDef?.RuleElement != null)
                {
                    languageRuleDef.RuleElement.CollectAllDefs(items);
                }
                
                foreach (RuleRepositoryDefCollection defs in parentDef.GetAllChildCollections())
                {
                    foreach (RuleRepositoryDefBase def in defs)
                    {
                        def.CollectAllDefs(items);
                    }
                }
            }
        }
        public static string GetDefTypeCountSummary(this RuleRepositoryDefBase parentDef)
        {
            var defItems = parentDef.GetAllDefs();
            var items = new List<string>();
            defItems.ForEach(r => items.AddRange(r.AppendDefDescriptions()));

            var results = from p in items
                group p by p into g
                select new { TypeDescription = g.Key, Count = g.Count() };




            var ordered = results.OrderBy(g => g.TypeDescription);

            return string.Join(Environment.NewLine,
                ordered.Select(t => t.TypeDescription.PadRight(50) + t.Count.ToString("#,##0").PadLeft(15)));

        }

        public static IEnumerable<string> AppendDefDescriptions(this RuleRepositoryDefBase def)
        {
            var className = def.GetType().GetDefTypeFullName();
            yield return className;
            if (def is FieldDef && !(def is FieldDefInfo))
            {
                var typed = (FieldDef)def;
                if (typed.IsCollection)
                {
                    className += " (Collection)";
                }

                className += $" ({typed.DataType})";

                if (typed.IsAnEntityDataType)
                {
                    if (typed.IsMarkedAsParentContext())
                    {
                        className += $" ({"Parent Context"})";
                    }

                }
                yield return className;
            }
            if (def is TemplateValueDef)
            {
                var templateValueDef = (TemplateValueDef) def;
                var compiledTypeName = templateValueDef.CompiledTypeName;
                var justClassName = compiledTypeName.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                var trailingName = justClassName.Split(new string[]{"."}, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                yield return $"{className}.{trailingName}";
            }
        }

        public static string GetDefTypeFullName(this Type type)
        {
            if (type == null) return null;
            if (type.Name == "Object") return null;
            if (type.Name == "RuleRepositoryDefBase") return type.BaseType.GetDefTypeFullName();
            if (type.Name == "DefTemplateBaseDef") return type.BaseType.GetDefTypeFullName();
            
            var baseName = type.BaseType.GetDefTypeFullName();
            if (baseName != null)
            {
                return $"{baseName}.{type.Name}";
            }
            else
            {
                return type.Name;
            }
        }


        public static bool IsMarkedAsParentContext(this FieldDef entityFieldDef)
        {

            if (entityFieldDef.DataType != DataType.Entity)
            {
                throw new ArgumentException("Field must be an entity field in order to mark as parent context");
            }
            // Parent context is stored as an implicit cascade on the embedded entity
            // The parent of the cascade is the parent entity, the child of the cascade is the embedded entity
            // The field that contains the cascade is not stored
            if (entityFieldDef.Parent is EntityDef)
            {
                var parentEntityDef = (EntityDef)entityFieldDef.Parent;
                if (parentEntityDef == null)
                {
                    throw new ArgumentException(
                        "Field must be added to an entity before is can be marked as parent context");
                }
                var childEntityDef = entityFieldDef.GetRuleApp().Entities[entityFieldDef.DataTypeEntityName];
                if (childEntityDef == null)
                {
                    throw new ArgumentException("Entity type '" + entityFieldDef.DataTypeEntityName +
                                                "' could not be found.  In order to create a parent context relationship, the entity type of the field must be a valid entity name");
                }
                return childEntityDef.CascadedReferences.Any(
                    r => r.IsImplicit && r.ParentId == parentEntityDef.Guid && r.ChildId == childEntityDef.Guid);
            }
            return false;
        }
    }

    public class UpdatedDefInfo
    {
        public RuleRepositoryDefBase TargetDef;
        public List<RuleRepositoryDefBase> UpdatedBy = new List<RuleRepositoryDefBase>();
        public bool IsCalculation { get; set; }
    }

    public class ConsumedDefInfo
    {
        public RuleRepositoryDefBase TargetDef;
        public List<RuleRepositoryDefBase> ConsumedBy = new List<RuleRepositoryDefBase>();
    }
}