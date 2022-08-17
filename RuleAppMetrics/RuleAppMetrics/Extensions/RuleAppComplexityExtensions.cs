using InRule.Authoring.Services;
using InRule.Repository;
using InRule.Repository.Client;
using InRule.Repository.DecisionTables;
using InRule.Repository.EndPoints;
using InRule.Repository.Infos;
using InRule.Repository.RuleElements;
using InRule.Repository.UDFs;
using InRule.Runtime;
using InRuleLabs.AuthoringExtensions.RuleAppMetrics.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InRuleLabs.AuthoringExtensions.RuleAppMetrics.Extensions
{
    internal static class RuleAppComplexityExtensions
    {
        //public static FactRuleApplication CalculateRuleAppComplexity(this RuleApplicationReference ruleAppRef)
        public static FactRuleApplication CalculateRuleAppComplexity(this RuleApplicationDef def, PersistenceInfo persistance)
        {
            //var def = ruleAppRef.GetRuleApplicationDef();

            // SECTION: Calculate RuleApplication Fact
            var ruleAppMetrics = new FactRuleApplication();

            Console.WriteLine("Collecting rule application definition size metrics...");
            #region Basic RuleAppDef metrics
            ruleAppMetrics.RuleAppName = def.Name;

            if (persistance.OpenedFrom == RuleAppOpenedFrom.FileSystem)
            {
                ruleAppMetrics.RuleAppSource = persistance.Filename;
            }
            else if (persistance.OpenedFrom == RuleAppOpenedFrom.New)
            {
                ruleAppMetrics.RuleAppSource = "New";
            }
            else if (persistance.IsCatalog)
            {
                ruleAppMetrics.RuleAppSource = persistance.CatalogName + ": " + persistance.ServiceUri;
                ruleAppMetrics.RuleAppRevision = def.Revision;
            }

            ruleAppMetrics.RuleAppDefSizeMB = def.GetXml().Length / 1000000.0;
            ruleAppMetrics.EntityCount = def.Entities.Count;
            foreach (EntityDef entity in def.Entities)
            {
                ruleAppMetrics.FieldCount += entity.Fields.Count;
                ruleAppMetrics.TemporaryFieldCount += entity.Fields.Where(f => f.StateLocation == StateLocation.TemporaryState).Count();
                ruleAppMetrics.CalculatedFieldCount += entity.Fields.Where(f => f.IsCalculated).Count();
                ruleAppMetrics.CollectionFieldCount += entity.Fields.Where(f => f.IsCollection).Count();

                var ruleSets = entity.GetAllRuleSets();
                ruleAppMetrics.RuleSetCount += ruleSets.Count();

                foreach (var ruleSet in ruleSets)
                {
                    ruleAppMetrics.RuleCount += ruleSet.GetAllRuleElements().ToList().Count();

                    if (ruleSet.FireMode == RuleSetFireMode.Auto && ruleSet.RunMode != RuleSetRunMode.SequentialRunOnce)
                        ruleAppMetrics.MultiPassRuleSetCount++;
                }
            }
            if (def.EndPoints != null && def.EndPoints.Count > 0)
            {
                foreach (EndPointDef dataElement in def.EndPoints)
                {
                    if (dataElement is RestServiceDef red)
                    {
                        ruleAppMetrics.ExternalEndpointCount++;
                    }
                    else if (dataElement is WebServiceDef wsd)
                    {
                        ruleAppMetrics.ExternalEndpointCount++;
                    }
                    else if (dataElement is DatabaseConnection dc)
                    {
                        ruleAppMetrics.ExternalEndpointCount++;
                    }
                }
            }
            foreach (UdfLibraryDef udfLibrary in def.UdfLibraries)
            {
                ruleAppMetrics.UdfCount += udfLibrary.UserDefinedFunctions.Count;
                ruleAppMetrics.UdfWithStateRefreshCount += udfLibrary.UserDefinedFunctions.Where(udf => udf.RuleWriteAll).Count();
            }
            #endregion

            Console.WriteLine("Calculating dependancy network...");
            #region Capture ConsumedBy/UpdatedBy/ExecutedBy counts
            var networkElements = new List<DependancyNetworkElement>();
            var network = InRule.Repository.Infos.DefUsageNetwork.Create(def);
            foreach (EntityDef entity in def.Entities)
            {
                foreach (FieldDef field in entity.Fields)
                {
                    var fieldDNE = GetDNE(field, networkElements);
                    var usages = network.GetDefUsages(field.Guid, true);
                    foreach (var usage in usages)
                    {
                        if (usage.UsageType == InRule.Repository.Infos.DefUsageType.ConsumedBy)
                        {
                            var elementDef = def.LookupItem(usage.Element);
                            var elementDNE = GetDNE(elementDef, networkElements);
                            fieldDNE.ConsumedBy.Add(elementDNE);
                        }
                        else if (usage.UsageType == InRule.Repository.Infos.DefUsageType.UpdatedBy)
                        {
                            var elementDef = def.LookupItem(usage.Element);
                            var elementDNE = GetDNE(elementDef, networkElements);
                            fieldDNE.UpdatedBy.Add(elementDNE);
                        }
                        //These won't cover collection member actions
                    }
                }
                foreach (RuleSetDef ruleSet in entity.GetRuleSets().Where(rs => rs.FireMode == RuleSetFireMode.Explicit))
                {
                    var rulesetDNE = GetDNE(ruleSet, networkElements);
                    var usages = network.GetDefUsages(ruleSet.Guid, true);
                    foreach (var usage in usages)
                    {
                        if (usage.UsageType == InRule.Repository.Infos.DefUsageType.ExecutedBy)
                        {
                            var elementDef = def.LookupItem(usage.Element);
                            var elementDNE = GetDNE(elementDef, networkElements);
                            rulesetDNE.FiredBy.Add(elementDNE);
                        }
                    }
                }
            }
            foreach (EndPointDef dataElement in def.EndPoints)
            {
                if (dataElement is RestServiceDef red || dataElement is WebServiceDef wsd || dataElement is DatabaseConnection dc)
                {
                    var udfDNE = GetDNE(dataElement, networkElements);
                    var usages = network.GetDefUsages(dataElement.Guid, true);
                    foreach (var usage in usages)
                    {
                        if (usage.UsageType == InRule.Repository.Infos.DefUsageType.ConsumedBy)
                        {
                            ruleAppMetrics.ExternalEndpointConsumers++;
                        }
                    }
                }
            }
            foreach (UdfLibraryDef udfLibrary in def.UdfLibraries)
            {
                foreach (UdfDef udf in udfLibrary.UserDefinedFunctions)
                {
                    var udfDNE = GetDNE(udf, networkElements);
                    var usages = network.GetDefUsages(udf.Guid, true);
                    foreach (var usage in usages)
                    {
                        if (usage.UsageType == InRule.Repository.Infos.DefUsageType.ConsumedBy)
                        {
                            ruleAppMetrics.UdfConsumers++;
                            if (udf.RuleWriteAll)
                                ruleAppMetrics.UdfWithStateRefreshConsumers++;
                        }
                    }
                }
            }
            ruleAppMetrics.FieldConsumedByDependancies = networkElements.Sum(e => e.ConsumedBy.Count);
            ruleAppMetrics.FieldUpdatedByDependancies = networkElements.Sum(e => e.UpdatedBy.Count);
            ruleAppMetrics.RuleSetFiredByDependancies = networkElements.Sum(e => e.FiredBy.Count);
            #endregion

            PopulateFieldUsageSummary(def, ruleAppMetrics);

            Console.WriteLine("Testing memory consumption and compile time...");
            #region Compile RuleApp and test times and memory usage
            var compilationTimer = new Stopwatch();
            long initialWorkingM = -1;
            try
            {
                // Get InRule bits loaded into memory
                var dummyRuleDef = new RuleApplicationDef();
                var dummyRuleApp = new InMemoryRuleApplicationReference(dummyRuleDef);
                dummyRuleApp.Compile();

                // Reset statistics
                GC.Collect();
                initialWorkingM = Process.GetCurrentProcess().WorkingSet64;

                //Perform the compilation
                var tempRuleAppRef = new InMemoryRuleApplicationReference(def);
                compilationTimer.Start();
                tempRuleAppRef.Compile(CacheRetention.Default, CompileSettings.Create(EngineLogOptions.None, Environment.ProcessorCount));
                compilationTimer.Stop();

                // Collect and report metrics
                var currentProcess = Process.GetCurrentProcess();
                GC.Collect();
                ruleAppMetrics.RuleAppCompiledSizeMB = (currentProcess.WorkingSet64 - initialWorkingM) / 1000000.0;
                ruleAppMetrics.RuleAppCompilationTimeSec = compilationTimer.ElapsedMilliseconds / 1000.0;
            }
            catch (Exception ex)
            {
                // Try to give an estimate for how long we spent compiling if things fail
                try
                {
                    ruleAppMetrics.RuleAppCompilationTimeSec = compilationTimer.ElapsedMilliseconds / 1000.0;
                    if (initialWorkingM > -1)
                    {
                        var currentProcess = Process.GetCurrentProcess();
                        ruleAppMetrics.RuleAppCompiledSizeMB = (currentProcess.WorkingSet64 - initialWorkingM) / 1000000.0;
                    }
                }
                catch { }

                Console.WriteLine("Cannot collect compilation statistics, RuleAppCompilationTimeSec and RuleAppCompiledSizeMB may not be accurate: " + ex.Message);
                Console.WriteLine("Full Error: " + ex.ToString());
            }
            #endregion

            Console.WriteLine("Rule Application Data:");
            Console.WriteLine(JsonConvert.SerializeObject(ruleAppMetrics, Formatting.Indented));
            Console.WriteLine("");

            return ruleAppMetrics;
        }
        private static void PopulateFieldUsageSummary(RuleApplicationDef ruleAppDef, FactRuleApplication ruleAppMetrics)
        {
            var allFields = new Stack<RuleRepositoryDefBase>();
            foreach (var entityDef in ruleAppDef.Entities.OfType<EntityDef>())
            {
                foreach (var fieldDef in entityDef.Fields.OfType<FieldDef>())
                {
                    allFields.Push(fieldDef);
                }
            }
            var unusedFields = allFields.ToList();

            var network = DefUsageNetwork.Create(ruleAppDef);

            var consumedFields = new List<RuleRepositoryDefBase>();
            var updatedFields = new List<RuleRepositoryDefBase>();
            foreach (var entityDef in ruleAppDef.Entities.OfType<EntityDef>())
            {
                foreach (var fieldDef in entityDef.Fields.OfType<FieldDef>())
                {
                    if (fieldDef.IsCalculated)
                    {
                        if (!updatedFields.Any(x => x == fieldDef))
                        {
                            updatedFields.Add(fieldDef);
                        }
                    }
                    else
                    {
                        var usages = network.GetDefUsages(fieldDef.Guid, false);
                        foreach (var usage in usages)
                        {
                            if (usage.UsageType == DefUsageType.UpdatedBy)
                            {
                                var defInfo = updatedFields.FirstOrDefault(x => x == fieldDef);
                                if (defInfo == null)
                                {
                                    updatedFields.Add(fieldDef);
                                }
                            }
                            else if (usage.UsageType == DefUsageType.ConsumedBy)
                            {
                                var defInfo = consumedFields.FirstOrDefault(x => x == fieldDef);
                                if (defInfo == null)
                                {
                                    consumedFields.Add(fieldDef);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var field in consumedFields)
            {
                unusedFields.Remove(field);
            }
            foreach (var field in updatedFields)
            {
                unusedFields.Remove(field);
            }

            ruleAppMetrics.ConsumedFieldCount = consumedFields.Count();
            ruleAppMetrics.UpdatedFieldCount = updatedFields.Count();
            ruleAppMetrics.UnusedFieldCount = unusedFields.Count();
        }
        private static DependancyNetworkElement GetDNE(RuleRepositoryDefBase element, List<DependancyNetworkElement> elements)
        {
            var existingElement = elements.FirstOrDefault(e => e.Id == element.Guid);
            if (existingElement == null)
            {
                existingElement = new DependancyNetworkElement(element);
                elements.Add(existingElement);
            }
            return existingElement;
        }

        public static FactRuleExecution CalculateRuleAppExecutionMetrics(this RuleApplicationReference ruleAppRef, string rootEntityName, string initialEntityStateFilePath, string explicitRuleSetName = null)
        {
            Console.WriteLine("Executing Rules for sample run...");
            RuleExecutionLog log = null;
            #region Execute Rules
            string initialEntityState = File.ReadAllText(initialEntityStateFilePath);
            EntityStateType fileContentType = initialEntityStateFilePath.ToLower().Contains("xml") ? EntityStateType.Xml : EntityStateType.Json;
            try
            {
                using (var session = new RuleSession(ruleAppRef))
                {
                    session.Settings.LogOptions = EngineLogOptions.SummaryStatistics | EngineLogOptions.DetailStatistics | EngineLogOptions.Execution;

                    Entity entity = session.CreateEntity(rootEntityName, initialEntityState, fileContentType);

                    if (explicitRuleSetName == null)
                        session.ApplyRules();
                    else
                        entity.ExecuteRuleSet(explicitRuleSetName);

                    log = session.LastRuleExecutionLog;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot run test Rule Execution: " + ex.Message);
            }
            #endregion

            var ruleExecutionMetrics = new FactRuleExecution();
            var def = ruleAppRef.GetRuleApplicationDef();

            #region Basic Execution Info
            ruleExecutionMetrics.HostMachine = Environment.MachineName;
            ruleExecutionMetrics.RuleAppName = def.Name;
            ruleExecutionMetrics.RootEntityName = rootEntityName;
            ruleExecutionMetrics.RuleSetName = explicitRuleSetName ?? "Auto";
            ruleExecutionMetrics.RuleAppCacheCount = RuleSession.RuleApplicationCache.Count;
            ruleExecutionMetrics.RuleAppCacheUptimeMinutes = RuleSession.RuleApplicationCache.Uptime.TotalMinutes.Round(5);
            ruleExecutionMetrics.ExecutionFinishedTime = DateTime.UtcNow;

            if (ruleAppRef is CatalogRuleApplicationReference crr2)
            {
                ruleExecutionMetrics.RuleAppSource = crr2.CatalogUri.AbsoluteUri;
                ruleExecutionMetrics.RequestedRuleAppRevision = crr2.RuleApplicationVersion.Revision;
                ruleExecutionMetrics.RequestedRuleAppLabel = crr2.RuleApplicationVersion.Label;
                ruleExecutionMetrics.RuleAppRevision = def.Revision;
            }
            else if (ruleAppRef is FileSystemRuleApplicationReference frr2)
            {
                ruleExecutionMetrics.RuleAppSource = frr2.Path;
            }
            #endregion

            if (log != null)
            {
                Console.WriteLine("Collecting data from execution log...");
                ruleExecutionMetrics.ImportExecutionLog(log); // Requires LogOptions.SummaryStats - and maybe DetailStat?

                #region Calculate counts of rules that were interacted with - requires LogOptions.Execution
                //Load list of all Rule IDs
                List<string> unhitRules = new List<string>();
                LoadAllRuleIdentifiersForFact(def.Entities[rootEntityName], unhitRules);
                ruleExecutionMetrics.TotalRuleElementCount = unhitRules.Count();

                //Update list to remove Rules that have been hit
                foreach (var logMessage in log.AllMessages.Select(m => m.Description).Where(m => m.StartsWith("Action")))
                {
                    var entitySeparatorRegex = new Regex(@"[:]*[\d]*/");
                    var path = logMessage.Substring(logMessage.IndexOf('\"') + 1).Split('\"')[0];
                    path = entitySeparatorRegex.Replace(path, ".");

                    var previouslyUnhitRule = unhitRules.FirstOrDefault(r => r == path);
                    if (previouslyUnhitRule != null)
                        unhitRules.Remove(previouslyUnhitRule);
                }

                // Calculate rule usage information
                ruleExecutionMetrics.HitRuleElementCount = ruleExecutionMetrics.TotalRuleElementCount - unhitRules.Count();
                #endregion
            }

            Console.WriteLine("Rule Execution Data:");
            Console.WriteLine(JsonConvert.SerializeObject(ruleExecutionMetrics, Formatting.Indented));
            Console.WriteLine("");

            return ruleExecutionMetrics;
        }
        private static void LoadAllRuleIdentifiersForFact(EntityDef entity, List<string> ruleNames)
        {
            foreach (var element in entity.GetAllRuleElements().Where(e => e.RuleElementType != RuleElementType.RuleSet && e.RuleElementType != RuleElementType.RuleSetFolder))
            {
                if (element is DecisionTableDef dtd)
                {
                    var dtName = element.Name;
                    //If it has the default name of DecisionTable1, it uses just DecisionTable, otherwise uses the proper name.  Based on reverse engineering, needs further testing
                    if (dtName.StartsWith("DecisionTable"))
                        dtName = "DecisionTable";

                    var locationOfLastDot = element.AuthoringElementPath.LastIndexOf('.');
                    var dtPath = element.AuthoringElementPath.Substring(0, locationOfLastDot) + "." + dtName + ".";

                    foreach (ActionDimensionDef action in dtd.Actions)
                    {
                        int index = 1;
                        foreach (ActionValueDef actionValue in action.ActionValues)
                        {
                            //This is weird and based on reverse engineering, needs further testing.
                            var actionName = action.FieldName + "_" + index++;
                            if (actionValue.RuleElement == null)
                                actionName = actionValue.ActionType + "_" + actionName;

                            ruleNames.Add(dtPath + actionName);
                        }
                    }
                }
                else
                {
                    ruleNames.Add(element.AuthoringElementPath);
                }
            }

            foreach (FieldDef field in entity.Fields)
            {
                if (field.IsAnEntityDataType)
                {
                    EntityDef fieldEntity = entity.GetRuleApp().Entities[field.DataTypeEntityName];
                    LoadAllRuleIdentifiersForFact(fieldEntity, ruleNames);
                }
            }
        }

        public static double Round(this double value, int places)
        {
            return Math.Round(value, places);
        }
    }
}