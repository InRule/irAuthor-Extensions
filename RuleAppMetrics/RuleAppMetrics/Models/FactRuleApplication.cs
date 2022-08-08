using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InRuleLabs.AuthoringExtensions.RuleAppMetrics.Models
{
    internal class FactRuleApplication
    {
        public string RuleAppSource;
        public int? RuleAppRevision;
        public string RuleAppName;

        // Def sizing info
        public int EntityCount;
        public int FieldCount;
        public int TemporaryFieldCount;
        public int CalculatedFieldCount;
        public int CollectionFieldCount;
        public int RuleSetCount;
        public int MultiPassRuleSetCount;
        public int RuleCount;
        public int ExternalEndpointCount;
        public int UdfCount;
        public int UdfWithStateRefreshCount;
        public int ConsumedFieldCount;
        public int UpdatedFieldCount;
        public int UnusedFieldCount;

        // Rule App scale info
        public double RuleAppDefSizeMB;
        public double RuleAppCompiledSizeMB;
        public double RuleAppCompilationTimeSec;

        // Dependancy Network data
        public int FieldUpdatedByDependancies;
        public int FieldConsumedByDependancies;
        public int RuleSetFiredByDependancies;
        public int ExternalEndpointConsumers;
        public int UdfConsumers;
        public int UdfWithStateRefreshConsumers;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
