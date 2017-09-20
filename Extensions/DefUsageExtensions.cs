using System;
using System.Collections.Generic;
using System.Linq;
using InRule.Repository;
using InRule.Repository.Infos;
using InRule.Repository.RuleElements;

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