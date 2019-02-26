using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using InRule.Repository;
using InRule.Repository.Client;
using InRule.Repository.DecisionTables;
using InRule.Repository.RuleElements;
using InRule.Repository.Service.Data;
using InRule.Runtime;

namespace InRule.RuleApplicationFramework
{
    public static class RuleEngineUtil
    {
        //hack when we lose ability to obtain ruleappdef deep in def model (ie within decision table action defs ) 
        private static RuleApplicationDef _workingRuleAppDef = null;

        #region IO related
        public static IEnumerable<string> GetRuleApplicationListFromFileSystem(string directoryPath)
        {
            var ruleApps = new List<string>();
            var dirInfo = new DirectoryInfo(directoryPath);
            dirInfo.GetFiles("*.ruleapp").ToList().ForEach(r => ruleApps.Add(CaseInsensitiveStringReplace(r.Name, ".ruleapp", "")));

            return ruleApps;
        }

        public static IEnumerable<string> GetRuleApplicationListFromCatalog(RuleExecutionInfo ruleExecutionInfo)
        {
            var ruleApps = new List<string>();

            using (var conn = GetCatalogConnection(ruleExecutionInfo))
            {
                conn.GetAllRuleApps().ToList().ForEach(r => ruleApps.Add(r.Key.Name));
            }

            return ruleApps;
        }

        public static RuleCatalogConnection GetCatalogConnection(RuleExecutionInfo ruleExecutionInfo)
        {
            return new RuleCatalogConnection(new Uri(ruleExecutionInfo.CatalogUri), new TimeSpan(0, 0, 60), ruleExecutionInfo.Username, ruleExecutionInfo.Password);
        }

        public static FileSystemRuleApplicationReference GetFileRuleAppReference(RuleExecutionInfo ruleExecutionInfo)
        {
            return new FileSystemRuleApplicationReference(ruleExecutionInfo.RuleAppFilePath);
        }

        public static InMemoryRuleApplicationReference GetInMemoryRuleAppReference(RuleApplicationDef ruleApplicationDef)
        {
            return new InMemoryRuleApplicationReference(ruleApplicationDef);
        }

        public static CatalogRuleApplicationReference GetCatalogRuleAppReference(RuleExecutionInfo ruleExecutionInfo)
        {
            var ruleapp = new CatalogRuleApplicationReference(ruleExecutionInfo.CatalogUri, ruleExecutionInfo.RuleAppName, ruleExecutionInfo.Username, ruleExecutionInfo.Password);
            ruleapp.SetRefresh(ruleExecutionInfo.CatalogRefreshInterval);
            ruleapp.ConnectionTimeout = ruleExecutionInfo.CatalogRuleAppTimeoutInterval;
            return ruleapp;
        }

        public static RuleApplicationDef GetRuleApplicationDefFromFile(RuleExecutionInfo ruleExecutionInfo)
        {
            return RuleApplicationDef.Load(ruleExecutionInfo.RuleAppFilePath);
        }

        public static RuleApplicationDef GetRuleApplicationDefFromCatalog(RuleExecutionInfo ruleExecutionInfo)
        {
            return GetCatalogRuleAppReference(ruleExecutionInfo).GetRuleApplicationDef();
        }
        
        public static string CaseInsensitiveStringReplace(string original, string search, string replaceWith)
        {
            if (original == null)
            {
                return "";
            }
            else
            {
                // building case insensitive replace regex pattern
                var s = new StringBuilder();
                foreach (var c in search.ToCharArray())
                {
                    s.Append("[");
                    s.Append(c.ToString().ToUpper());
                    s.Append(c.ToString().ToLower());
                    s.Append("]");
                }

                return Regex.Replace(original, s.ToString(), replaceWith);
            }
        }

        public static void RemoveRuleAppFromCache(string ruleAppName)
        {
            // NOTE: This does not account for multiple revisions of the same named rule app
            foreach (var ruleApplicationReference in RuleSession.RuleApplicationCache.Items)
            {
                if (ruleApplicationReference.GetRuleApplicationDef().Name == ruleAppName)
                {
                    RuleSession.RuleApplicationCache.Remove(ruleApplicationReference);
                    break;
                }
            }
        }

        #endregion

        #region entity related
        //public static IEnumerable<string> GetEntityNames(RuleApplicationDef ruleAppDef)
        //{
        //    ruleAppDef.Entities
        //}

        public static void ActivateRuleSetsByCategory(this RuleSession ruleSession, string categoryName)
        {
            foreach (var entity in ruleSession.GetEntities())
            {
                var ruleSets = entity.RuleSets.Where(r => r.Categories.Contains(categoryName)).ToList();
                ruleSets.ForEach(r => r.IsActivated = true);
            }
        }

        public static bool IsEntityObjectBound(RuleApplicationDef ruleAppDef, string entityName)
        {
            // if null, the entity is not object bound
            return ruleAppDef.Entities[entityName].BoundEntityTypeInfo != null;
        }
        
        #endregion

        #region Repository Methods
        public static FileInfo[] GetRuleApplicationFileInfos(string directoryPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            return dirInfo.GetFiles("*.ruleapp");
        }

        public static RuleApplicationDef GetRuleApplicationDef(string filePath)
        {
            return RuleApplicationDef.Load(filePath);
        }

        public static RuleApplicationDef GetRuleApplicationDef(RuleCatalogConnection conn, string ruleAppName)
        {
            RuleAppRef appRef = conn.GetRuleAppRef(ruleAppName);
            return conn.GetSpecificRuleAppRevision(appRef.Guid, appRef.PublicRevision);
        }

        public static RuleApplicationDef GetRuleApplicationDef(RuleCatalogConnection conn, string ruleAppName, string labelName)
        {
            RuleAppRef appRef = conn.GetRuleAppRef(ruleAppName, labelName);
            return conn.GetSpecificRuleAppRevision(appRef.Guid, appRef.PublicRevision);
        }

        public static RuleSetDef GetRuleSetDef(string ruleSetName, EntityDef entityDef)
        {
            //this ensure that rulesets in rule folders are included
            return entityDef.GetAllRuleSets().First((r => r.Name == ruleSetName));
        }

        public static RuleElementDef GetRuleElementDef(string ruleElementName, RuleElementDef parentDef)
        {
            RuleElementDef ruleDef = null;

            if (parentDef is LanguageRuleDef)
            {
                if (((LanguageRuleDef)parentDef).RuleElement.Name == ruleElementName)
                {
                    ruleDef = ((LanguageRuleDef)parentDef).RuleElement;
                }
            }
            else
            {
                RuleElementDefCollection ruleDefs = GetRuleElementDefCollection(parentDef);
                ruleDef = (RuleElementDef)ruleDefs[ruleElementName];
            }

            return ruleDef;
        }

        public static RuleElementDef GetLanguageRuleElementDef(RuleElementDef ruleDef)
        {
            
            if (ruleDef is LanguageRuleDef)
            {
                return ((LanguageRuleDef)ruleDef).RuleElement;
            }
            return null;
        }

        public static RuleElementDefCollection GetRuleElementDefCollection(RuleElementDef parentDef)
        {
            RuleElementDefCollection ruleDefs = null;

            if (parentDef is RuleSetDef)
            {
                ruleDefs = ((RuleSetDef)parentDef).Rules;
            }
            else if (parentDef is SimpleRuleDef)
            {
                ruleDefs = ((SimpleRuleDef)parentDef).SubRules;
            }
            else if (parentDef is ExclusiveRuleDef)
            {
                ruleDefs = ((ExclusiveRuleDef)parentDef).Conditions;
            }

            return ruleDefs;
        }
        
        public static RuleSetDef GetRuleSetByName(RuleApplicationDef ruleAppDef, string ruleSetName)
        {
            RuleSetDef ruleSetDef = null;

            // first look in Entities for the RuleSet
            foreach (EntityDef entityDef in ruleAppDef.Entities)
            {
                ruleSetDef = GetRuleSetByName(entityDef, ruleSetName);
                if (ruleSetDef != null)
                {
                    return ruleSetDef;
                }
            }
            // if we haven't found it, look in the independent rulesets
            foreach (RuleSetDef indRuleSetDef in ruleAppDef.GetAllRuleSets())
            {
                if (indRuleSetDef.Name == ruleSetName)
                {
                    return indRuleSetDef;
                }
            }

            return ruleSetDef;
        }

        public static RuleSetDef GetRuleSetByName(EntityDef entityDef, string ruleSetName)
        {
            foreach (RuleSetDef ruleSetDef in entityDef.GetAllRuleSets())
            {
                if (ruleSetDef.Name == ruleSetName)
                {
                    return ruleSetDef;
                }

            }
            return null;
        }

        public static string GetMemberRuleSetByName(RuleApplicationDef ruleAppDef, string ruleSetName)
        {
            var ruleSetDef = GetRuleSetByName(ruleAppDef, ruleSetName);

            return GetMemberRuleSetByName(ruleSetDef, ruleSetName);
        }

        public static string GetMemberRuleSetByName(EntityDef entityDef, string ruleSetName)
        {
            var ruleSetDef = GetRuleSetByName(entityDef, ruleSetName);

            return GetMemberRuleSetByName(ruleSetDef, ruleSetName);
        }

        public static string GetMemberRuleSetByName(RuleSetDef parentRuleSetDef, string ruleSetName)
        {
            string memberRuleSetName = string.Empty;

            //TODO: this doesn't process each rule, just the last one 
            foreach (var rule in parentRuleSetDef.Rules)
            {
                if (rule is ExecuteMemberRuleSetActionDef)
                {
                    // if this is Execute Member action, get the name of the RuleSet so we can find it
                    memberRuleSetName = ((ExecuteMemberRuleSetActionDef)rule).RuleSet.FormulaText;
                }
            }

            return memberRuleSetName;
        }

        public static RuleSetDef[] GetExplicitRuleSetDefs(RuleApplicationDef ruleAppDef, string entityName)
        {
            ArrayList ruleSetDefs = new ArrayList();
            EntityDef entityDef = ruleAppDef.Entities[entityName];

            if (entityDef != null)
            {
                foreach (var ruleSetDef in entityDef.GetAllRuleSets())
                {
                    if (ruleSetDef.FireMode == RuleSetFireMode.Explicit && ruleSetDef.IsActive)
                        ruleSetDefs.Add(ruleSetDef);
                }
            }
            return (RuleSetDef[])ruleSetDefs.ToArray(typeof(RuleSetDef));
        }

        public static EntityDef GetRootEntityDef(RuleApplicationDef ruleApplicationDef)
        {
            foreach (EntityDef entityDef in ruleApplicationDef.Entities)
            {
                if (!entityDef.GetIsReferencedByOtherEntities(false))
                {
                    return entityDef;
                }
            }
            return null;
        }
  
        public static string GetRuleExecutionFlowXml(RuleSetDef ruleSetDef)
        {
            string xml = null;
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);

            //TODO: this hack is described on the variable definition
            _workingRuleAppDef = ruleSetDef.GetRuleApp();

            using (xw)
            {
                xw.WriteStartElement(ruleSetDef.Name);

                foreach (RuleRepositoryDefBase def in ruleSetDef.Rules)
                {
                    //xw.WriteRaw("<Collection3>");
                    AppendToExecutionFlowXml(def as RuleElementDef, xw);
                    //xw.WriteRaw("</Collection3>");
                }
                xw.WriteEndElement();

                xml = sw.ToString();
            }

            _workingRuleAppDef = null;

            return xml;

        }

        public static void AppendToExecutionFlowXml(RuleElementDef def, XmlTextWriter xw)
        {
            if (def != null && def.IsActive)
            {
                if (def is ExecuteActionDef)
                {
                    AppendExecuteRuleSetTarget(def as ExecuteActionDef, xw);
                }
                else if (def is ExecuteMemberRuleSetActionDef)
                {
                    AppendExecuteMemberRuleSetTarget(def as ExecuteMemberRuleSetActionDef, xw);
                }
                else if (def is ExclusiveRuleDef)
                {
                    foreach (SimpleRuleDef simpleRuleDef in ((ExclusiveRuleDef)def).Conditions)
                    {
                        //xw.WriteRaw("<Collection4>");
                        AppendToExecutionFlowXml(simpleRuleDef, xw);
                        //xw.WriteRaw("</Collection4>");
                    }
                    foreach (RuleElementDef ruleDef in ((ExclusiveRuleDef)def).DefaultSubRules)
                    {
                        //xw.WriteRaw("<Collection6>");
                        AppendToExecutionFlowXml(ruleDef, xw);
                        //xw.WriteRaw("</Collection6>");
                    }
                }
                else if (def is DecisionTableDef)
                {
                    DecisionTableDef dtDef = def as DecisionTableDef;
                    //xw.WriteStartElement(dtDef.Name);
                    foreach (ActionDimensionDef aDef in dtDef.Actions)
                    {
                        //xw.WriteRaw("<" + aDef.DisplayName + ">");
                        foreach (ActionValueDef actionDef in aDef.ActionValues)
                        {
                            //xw.WriteRaw("<Collection>");
                            AppendToExecutionFlowXml(actionDef.RuleElement as RuleElementDef, xw);
                            //xw.WriteRaw("</Collection>");
                        }
                        //xw.WriteRaw("</" + aDef.DisplayName + ">");
                    }
                    //xw.WriteEndElement();
                }
                else if (def is IContainsRuleElements)
                {
                    foreach (RuleElementDef ruleElementDef in ((IContainsRuleElements)def).RuleElements)
                    {
                        //xw.WriteRaw("<Collection2>");
                        AppendToExecutionFlowXml(ruleElementDef, xw);
                        //xw.WriteRaw("</Collection2>");
                    }
                }
                else if (def is LanguageRuleDef)
                {
                        AppendToExecutionFlowXml(((LanguageRuleDef)def).RuleElement, xw);

                }
                //else if (def is FireNotificationActionDef)
                //{
                //    if (((FireNotificationActionDef)def).Name.Length > 0)
                //    {
                //        xw.WriteStartElement(((FireNotificationActionDef)def).Name);
                //        xw.WriteEndElement();
                //    }
                //    //AppendToExecutionFlowXml(((FireNotificationActionDef)def)., xw);

                //}
                //else if (def is AddCollectionMemberActionDef)
                //{
                //    if (((AddCollectionMemberActionDef)def).Name.Length > 0)
                //    {
                //        xw.WriteStartElement(((AddCollectionMemberActionDef)def).Name);
                //        xw.WriteEndElement();
                //    }
                //    //AppendToExecutionFlowXml(((FireNotificationActionDef)def)., xw);

                //}
                //else if (def is SetValueActionDef)
                //{
                //    if (((SetValueActionDef)def).Name.Length > 0)
                //    {
                //        xw.WriteStartElement(((SetValueActionDef)def).Name);
                //        xw.WriteEndElement();
                //    }
                //    //AppendToExecutionFlowXml(((FireNotificationActionDef)def)., xw);

                //}
                else
                {
                    //Console.WriteLine(def.ToString());
                }
            }

        }

        public static void AppendExecuteRuleSetTarget(ExecuteActionDef def, XmlTextWriter xw)
        {
            //cant always get the def when deep down, thus the workaround
            //var ruleSetDef = GetRuleSetByName(def.GetRuleApp(), def.TargetName);
            var ruleSetDef = GetRuleSetByName(_workingRuleAppDef, (def.TargetName.Contains(".")?def.TargetName.Substring(def.TargetName.LastIndexOf(".") + 1):def.TargetName));

            var targetName = def.TargetName.Contains(".") ? ruleSetDef.ThisEntity.Name + "." + def.TargetName.Substring(def.TargetName.LastIndexOf(".") + 1) : def.TargetName;
            if (ruleSetDef != null && ruleSetDef.IsActive)
            {
                //xw.WriteStartElement("ExecuteRuleSet");
                xw.WriteStartElement(targetName);

                //xw.WriteStartElement("_");

                foreach (RuleRepositoryDefBase ruleDef in ruleSetDef.Rules)
                {
                    //xw.WriteRaw("<Collection1>");
                    AppendToExecutionFlowXml(ruleDef as RuleElementDef, xw);
                    //xw.WriteRaw("</Collection1>");
                }

                xw.WriteEndElement();

                //xw.WriteEndElement();
            }

        }

        public static void AppendExecuteMemberRuleSetTarget(ExecuteMemberRuleSetActionDef def, XmlTextWriter xw)
        {
            var ruleSetDef = GetRuleSetByName(_workingRuleAppDef, def.RuleSetName);

            if (ruleSetDef != null && ruleSetDef.IsActive)
            {
                //xw.WriteStartElement("Execute_" + def.RuleSetName + "_ForEachMemberIn_" + def.CollectionName);
                var collectionEntityMemberName = string.Empty;
                if (def.CollectionName.Contains("."))
                {
                    var split = def.CollectionName.Split('.');
                    var entityName = split[split.Count() - 2];
                    var collectionFieldName = split[split.Count() - 1];

                    if (_workingRuleAppDef.Entities[entityName] != null)
                        collectionEntityMemberName = _workingRuleAppDef.Entities[entityName].Fields[collectionFieldName].DataTypeEntityName;
                    else
                    {
                        if (ruleSetDef.ThisEntity.Fields[collectionFieldName] != null)
                            collectionEntityMemberName = ruleSetDef.ThisEntity.Fields[collectionFieldName].DataTypeEntityName;
                        else if (def.ThisRuleSet.Parameters[entityName] != null)
                            collectionEntityMemberName = _workingRuleAppDef.Entities[def.ThisRuleSet.Parameters[entityName].DataTypeEntityName].Fields[collectionFieldName].DataTypeEntityName;
                    }
                }
                else
                    collectionEntityMemberName = def.Collection.ThisEntity.Fields[def.CollectionName].DataTypeEntityName;

                //Console.WriteLine (((InRule.Repository.RuleRepositoryDefBase)(((InRule.Repository.RuleElements.ExecuteMemberRuleSetActionDef)(def)).Collection)).ThisEntity.Name);
                //xw.WriteStartElement(def.RuleSetName + "_ForEach_" + def.Collection.ThisEntity.Fields[def.CollectionName].DataTypeEntityName);
                xw.WriteStartElement(def.RuleSetName + "_ForEach_" + collectionEntityMemberName);

                foreach (RuleRepositoryDefBase ruleDef in ruleSetDef.Rules)
                {
                    //xw.WriteRaw("<Collection>");
                    AppendToExecutionFlowXml(ruleDef as RuleElementDef, xw);
                    //xw.WriteRaw("</Collection>");
                }

                xw.WriteEndElement();
            }
        }

        #endregion

    
    }
}
