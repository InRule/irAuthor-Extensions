using System.Collections.Generic;

namespace ExtensionManager
{
    public class ExtensionManagerSettings
    {
        public List<string> EnabledExtensions { get; set; }

        public string FeedUrl { get; set; }
        public ExtensionManagerSettings()
        {
            EnabledExtensions = new List<string>();
            FeedUrl = "http://roadget.azurewebsites.net/nuget/"; //default
        }
    }
}