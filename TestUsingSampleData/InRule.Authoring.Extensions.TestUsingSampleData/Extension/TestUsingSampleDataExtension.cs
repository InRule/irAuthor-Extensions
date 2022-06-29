using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using InRule.Authoring.Commanding;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Controls;
using InRule.Authoring.Windows.Services;
using InRule.Runtime;

namespace InRule.Authoring.Extensions.TestUsingSampleData.Extension
{
    //TODO: Another option would be to build in the ability to manage Templates within the rule app schema (either via file or catalog) so they carry between Authors

    public class TestUsingSampleDataExtension : ExtensionBase
    {
        public new ServiceManager ServiceManager => base.ServiceManager;
        internal TestUsingSampleDataSettings Settings => TestUsingSampleDataSettings.Get(SettingsStorageService);
        private const string ExtensionGUID = "{18535651-86AA-4ACF-8E2C-9468CF89FAF6}";
        private bool _isEnabled = false;
        private IRibbonMenuButton _testMenuButton;

        public TestUsingSampleDataExtension()
            : base("Test Using Sample Data", "Allows irVerify to be launched with pre-filled data from JSON files on the local disk", new Guid(ExtensionGUID), false)
        {
        }

        public override void Enable()
        {
            try
            {
                if (_isEnabled) return;
                _isEnabled = true;

                AttachTestMenuListener();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Append("Error enabling NewFromTemplate extension: " + ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }
        private void AttachTestMenuListener()
        {
            try
            {
                var homeTab = IrAuthorShell.HomeTab;
                var ruleAppGroup = homeTab.GetGroup(Strings.Rule_Application);
                var panels = ruleAppGroup.Items.OfType<IRibbonStackPanel>();
                foreach (var panel in panels)
                {
                    foreach (var button in panel.Items.OfType<IRibbonMenuButton>())
                    {
                        if (button.Label == Strings.Test)
                        {
                            _testMenuButton = button;
                            _testMenuButton.PopupOpening += TestButton_PopupOpening;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Append("Error attaching Test menu listener: " + ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }
        private void TestButton_PopupOpening(object sender, EventArgs e)
        {
            try
            {
                // By default, use the data directory configured for this rule application
                string pullSampleDataFrom = Settings.GetDataDirectoryFor(RuleApplicationService.RuleApplicationDef.Name);

                // If that was not set, then try and get them from the same folder that the rule application resides in
                if(pullSampleDataFrom == null && RuleApplicationService.PersistenceInfo.OpenedFrom == RuleAppOpenedFrom.FileSystem)
                {
                    pullSampleDataFrom = Path.GetDirectoryName(RuleApplicationService.PersistenceInfo.Filename);
                }

                _testMenuButton.AddSeparator("Sample Data");

                if (pullSampleDataFrom != null)
                {
                    string[] directories = null;
                    try
                    {
                        directories = Directory.GetDirectories(pullSampleDataFrom);
                    }
                    catch(Exception ex)
                    {
                        LoggingService.Instance.Append($"Configured SampleData path {pullSampleDataFrom} was invalid.  " + ex.ToString());
                    }

                    if (directories != null)
                    {
                        foreach (var directory in directories)
                        {
                            var entityName = directory.Split('\\').Last();
                            foreach (var scenario in Directory.GetFiles(directory).Where(f => f.ToLower().EndsWith(".json")))
                            {
                                var scenarioName = Path.GetFileName(scenario);
                                scenarioName = scenarioName.Substring(0, scenarioName.Length - 5);
                                _testMenuButton.AddMenuItem(new VisualDelegateCommand(async delegate { await RunTestScenario(entityName, scenario); }, $"{entityName} : {scenarioName}"));
                            }
                        }
                    }
                }

                _testMenuButton.AddMenuItem(new VisualDelegateCommand(async delegate { await SelectSampleDataFolder(); }, $"Change Sample Data Folder..."));
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Append("Error handling Test menu popup opening event: " + ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }
        private async Task RunTestScenario(string rootEntity, string jsonSourcePath)
        {
            try
            {
                IIrAuthorShell shell;
                ServiceManager.TryGetService(out shell);

                //ensure the rule app has been saved prior to loading irVerify
                if (shell != null)
                {
                    shell.SaveContent();
                }
                
                var svc = this.ServiceManager.GetService<TestService>();
           
                var session = new RuleSession(this.RuleApplicationService.RuleApplicationDef);
                session.Settings.LogOptions = EngineLogOptions.Execution | EngineLogOptions.StateChanges | EngineLogOptions.SummaryStatistics | EngineLogOptions.DetailStatistics | EngineLogOptions.RuleValues;

                var data = File.ReadAllText(jsonSourcePath);
                var entity = session.CreateEntity(rootEntity, data, EntityStateType.Json);
                ServiceManager.GetService<TestService>().Test(entity);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Append("Error running test scenario: " + ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        private async Task SelectSampleDataFolder()
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Settings.SaveDataDirectoryFor(RuleApplicationService.RuleApplicationDef.Name, fbd.SelectedPath);
                }
            }
        }

        public override void Disable()
        {
            if (_isEnabled)
            {
                _isEnabled = false;

                try
                {
                    if (_testMenuButton != null)
                    {
                        _testMenuButton.PopupOpening += TestButton_PopupOpening;
                    }
                }
                catch(Exception ex)
                {
                    LoggingService.Instance.Append("Error disabling TestUsingSampleExtension: " + ex.ToString());
                }
            }
        }
    }
}