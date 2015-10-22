using System;

namespace ExtensionManager.ViewModels
{
    class ExtensionCommandEventArgs : EventArgs
    {
        public ExtensionCommandEventArgs(ExtensionRowViewModel extension)
        {
            Extension = extension;
        }

        public ExtensionRowViewModel Extension { get; set; }
    }
}