using System.Collections.Generic;

namespace ExtensionManager
{
    public class ExtensionManagerSettings
    {
        public List<string> InstalledExtensions { get; set; }

        public ExtensionManagerSettings()
        {
            InstalledExtensions = new List<string>();
        }
    }
}