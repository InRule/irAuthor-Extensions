using System.Collections.Generic;

namespace ExtensionManager
{
    public class ExtensionManagerSettings
    {
        public List<string> EnabledExtensions { get; set; }
        public ExtensionManagerSettings()
        {
            EnabledExtensions = new List<string>();
        }
    }
}