using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InRule.Authoring.Commanding;
using InRule.Authoring.Windows;

namespace ExtensionManager
{
    public class Extension : ExtensionBase
    {
        public VisualDelegateCommand ViewGalleryCommand { get; private set; }

        private const string RoadGetFeedUrl = "http://roadget.azurewebsites.net/nuget"; // TODO: move this into a runtime-configurable setting

        public Extension() 
            : base(name: "Extension Manager for IrAuthor", 
                  description: "Browse, manage, and install extensions from the extension gallery", 
                  guid: new Guid("{27B24F4A-E2FD-42D0-8B9F-639E99E72A35}"), 
                  isSystemExtension: true)
        {
        }

        public override void Enable()
        {
            ViewGalleryCommand = new VisualDelegateCommand(ViewGallery, "Extensions Manager", true);
        }

        private void ViewGallery(object obj)
        {
            
        }
    }
}
