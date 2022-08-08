using InRule.Runtime;
using InRuleLabs.AuthoringExtensions.RuleAppMetrics.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InRuleLabs.AuthoringExtensions.RuleAppMetrics.Models
{
    internal class FactRuleExecution
    {
        public string HostMachine;

        public string RuleAppSource;
        public string RuleAppName;
        public int? RuleAppRevision;
        public int? RequestedRuleAppRevision;
        public string RequestedRuleAppLabel;
        public string RootEntityName;
        public string RuleSetName;
        public int RuleAppCacheCount;
        public double RuleAppCacheUptimeMinutes;
        public DateTime ExecutionFinishedTime;

        public double RuleExecutionTimeMs;
        public double TotalExecutionTimeMs;
        public long CycleCount;
        public long StateValueChanges;
        public long TotalTraceFrames;
        public long ValidationChanges;

        public double MetadataCompileTimeMs;
        public double FunctionCompileTimeMs;
        public bool WasColdStart;
        public double TotalCalculationFieldEvaluationTimeMs;
        public double TotalActionExecutionTimeMs;
        public double TotalRuleEvaluationTimeMs;
        public int StateValueChangesCount;
        public int ActionsExecutedCount;
        public int CalculationFieldsEvaluatedCount;
        public int ActiveValidationsCount;
        public int ActiveNotificationsCount;
        public int TrueRulesCount;
        public int RuleSetsFiredCount;
        public int RulesEvaluatedCount;

        public double ExternalSmtpCallTimeMs;
        public int ExternalSmtpCallCount;
        public double ExternalMethodCallTimeMs;
        public int ExternalMethodCallCount;
        public double ExternalDataQueryCallTimeMs;
        public int ExternalDataQueryCallCount;
        public double ExternalWebServiceCallTimeMs;
        public int ExternalWebServiceCallCount;
        public double BoundStateRefreshTimeMs;
        public int BoundStateRefreshCount;
        public double ExternalWorkflowCallTimeMs;
        public int ExternalWorkflowCallCount;

        public bool HasErrors;
        public string ErrorMessages;

        public int TotalRuleElementCount;
        public int HitRuleElementCount;

        public void ImportExecutionLog(RuleExecutionLog log)
        {
            int numOfDecimalPlaces = 3;
            RuleExecutionTimeMs = log.RuleExecutionTime.TotalMilliseconds.Round(numOfDecimalPlaces);
            TotalExecutionTimeMs = log.TotalExecutionTime.TotalMilliseconds.Round(numOfDecimalPlaces);
            CycleCount = log.CycleCount;
            TotalTraceFrames = log.TotalTraceFrames;
            StateValueChanges = log.StateValueChanges.Count;
            ValidationChanges = log.ValidationChanges.Count;

            if (log.Statistics != null)
            {
                MetadataCompileTimeMs = log.Statistics.MetadataCompileTime.TotalMilliseconds.Round(numOfDecimalPlaces);
                FunctionCompileTimeMs = log.Statistics.FunctionCompileTime.RunningTotal.TotalMilliseconds.Round(numOfDecimalPlaces);
                WasColdStart = MetadataCompileTimeMs > 0 || FunctionCompileTimeMs > 0;
                TotalCalculationFieldEvaluationTimeMs = log.Statistics.TotalCalculationFieldEvaluationTime.TotalMilliseconds.Round(numOfDecimalPlaces);
                TotalActionExecutionTimeMs = log.Statistics.TotalActionExecutionTime.TotalMilliseconds.Round(numOfDecimalPlaces);
                TotalRuleEvaluationTimeMs = log.Statistics.TotalRuleEvaluationTime.TotalMilliseconds.Round(numOfDecimalPlaces);
                StateValueChangesCount = log.Statistics.StateValueChangesCount;
                ActionsExecutedCount = log.Statistics.ActionsExecutedCount;
                CalculationFieldsEvaluatedCount = log.Statistics.CalculationFieldsEvaluatedCount;
                ActiveValidationsCount = log.Statistics.ActiveValidationsCount;
                ActiveNotificationsCount = log.Statistics.ActiveNotificationsCount;
                TrueRulesCount = log.Statistics.TrueRulesCount;
                RuleSetsFiredCount = log.Statistics.RuleSetsFiredCount;
                RulesEvaluatedCount = log.Statistics.RulesEvaluatedCount;

                ExternalSmtpCallTimeMs = log.Statistics.ExternalSmtpCallTime.RunningTotal.TotalMilliseconds.Round(numOfDecimalPlaces);
                ExternalSmtpCallCount = log.Statistics.ExternalSmtpCallTime.SampleCount;
                ExternalMethodCallTimeMs = log.Statistics.ExternalMethodCallTime.RunningTotal.TotalMilliseconds.Round(numOfDecimalPlaces);
                ExternalMethodCallCount = log.Statistics.ExternalMethodCallTime.SampleCount;
                ExternalDataQueryCallTimeMs = log.Statistics.ExternalDataQueryCallTime.RunningTotal.TotalMilliseconds.Round(numOfDecimalPlaces);
                ExternalDataQueryCallCount = log.Statistics.ExternalDataQueryCallTime.SampleCount;
                ExternalWebServiceCallTimeMs = log.Statistics.ExternalWebServiceCallTime.RunningTotal.TotalMilliseconds.Round(numOfDecimalPlaces);
                ExternalWebServiceCallCount = log.Statistics.ExternalWebServiceCallTime.SampleCount;
                BoundStateRefreshTimeMs = log.Statistics.BoundStateRefreshTime.RunningTotal.TotalMilliseconds.Round(numOfDecimalPlaces);
                BoundStateRefreshCount = log.Statistics.BoundStateRefreshTime.SampleCount;
                ExternalWorkflowCallTimeMs = log.Statistics.ExternalWorkflowCallTime.RunningTotal.TotalMilliseconds.Round(numOfDecimalPlaces);
                ExternalWorkflowCallCount = log.Statistics.ExternalWorkflowCallTime.SampleCount;
            }

            HasErrors = log.HasErrors;
            if (log.HasErrors)
            {
                ErrorMessages = string.Join("|", log.ErrorMessages.Select(m => $"{m.SourceElementId}:{m.Exception}"));
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}