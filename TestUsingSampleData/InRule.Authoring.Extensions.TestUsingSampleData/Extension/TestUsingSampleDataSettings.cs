using InRule.Authoring.Services;
using InRule.Authoring.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InRule.Authoring.Extensions.TestUsingSampleData.Extension
{
    public class TestUsingSampleDataSettings : ISettings
    {
        public static Guid Guid = new Guid(@"{60B03104-0DAB-4EE6-A668-F9E360848A3D}");
        public Guid ID { get { return Guid; } }
        public string Description { get { return "Test Using Sample Data Options"; } }

        public List<SampleDataInfo> RuleAppSampleDataDirectories { get; set; }
        private SettingsStorageService _settingsStorageService;

        public TestUsingSampleDataSettings()
        {
            RuleAppSampleDataDirectories = new List<SampleDataInfo>();
        }

        public string GetDataDirectoryFor(string ruleAppName)
        {
            if(RuleAppSampleDataDirectories.Any(d => d.RuleAppName == ruleAppName))
            {
                return RuleAppSampleDataDirectories.First(d => d.RuleAppName == ruleAppName).DataDirectory;
            }
            return null;
        }
        public void SaveDataDirectoryFor(string ruleAppName, string dataDirectory)
        {
            var item = RuleAppSampleDataDirectories.FirstOrDefault(d => d.RuleAppName == ruleAppName);
            if(item == null)
            {
                RuleAppSampleDataDirectories.Add(new SampleDataInfo() {  RuleAppName = ruleAppName, DataDirectory = dataDirectory });
            }
            else
            {
                item.DataDirectory = dataDirectory;
            }
            _settingsStorageService.SaveSettings(this);
        }

        public static TestUsingSampleDataSettings Get(SettingsStorageService settingsStorageService)
        {
            var settings = settingsStorageService.LoadSettings<TestUsingSampleDataSettings>(TestUsingSampleDataSettings.Guid);
            settings._settingsStorageService = settingsStorageService;
            return settings;
        }
    }
    public class SampleDataInfo
    {
        public string RuleAppName { get; set; }
        public string DataDirectory { get; set; }
    }
}