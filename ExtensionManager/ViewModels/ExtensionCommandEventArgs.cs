using System;

namespace ExtensionManager.ViewModels
{
    class ExtensionCommandEventArgs : EventArgs
    {
        public ExtensionCommandEventArgs(string packageId = "")
        {
            ExtensionId = packageId;
        }

        public string ExtensionId { get; set; }
    }
}