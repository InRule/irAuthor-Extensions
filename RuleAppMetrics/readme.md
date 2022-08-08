# Show Rule Application Metrics

### Features
- Generates telemetry about the currently open Rule Application, which can be useful for troubleshooting, analysis, and performance tuning.

### Requirements
- IrAuthor version 5.2 or newer

## Installation
1. Download the source code and build the solution
2. Copy InRuleLabs.AuthoringExtensions.RuleAppMetrics.dll into the irAuthor Extensions directory or a subdirectory therein
3. When you launch irAuthor, enable the Extension, and the functionality will be included in the Home ribbon's App Analysis section under Metrics

## Usage
With a rule application open, click the "Metrics" button under the "App Analysis" section of the Home tab on the ribbon.  This may take a bit to generate, but will show a popup window with a variety of metrics about the rule application

### Rule Application Information
- RuleAppSource
- RuleAppRevision
- RuleAppName

### Def sizing info
- EntityCount
- FieldCount
- TemporaryFieldCount
- CalculatedFieldCount
- CollectionFieldCount
- RuleSetCount
- MultiPassRuleSetCount
- RuleCount
- ExternalEndpointCount
- UdfCount
- UdfWithStateRefreshCount
- ConsumedFieldCount
- UpdatedFieldCount
- UnusedFieldCount

### Rule App scale info
- RuleAppDefSizeMB
- RuleAppCompiledSizeMB (note: this is a rough estimate and may not reflect Production)
- RuleAppCompilationTimeSec (note: this is a rough estimate and may not reflect Production)

### Dependancy Network data
- FieldUpdatedByDependancies
- FieldConsumedByDependancies
- RuleSetFiredByDependancies
- ExternalEndpointConsumers
- UdfConsumers
- UdfWithStateRefreshConsumers