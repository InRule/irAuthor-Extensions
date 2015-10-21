using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ExtensionManager.Views;
using InRule.Authoring.Commanding;
using InRule.Authoring.Extensions;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;

namespace ExtensionManager
{
    public class Extension : ExtensionBase
    {
        public VisualDelegateCommand ViewGalleryCommand { get; private set; }

        
        public Extension() 
            : base(name: "Extension Manager for IrAuthor", 
                  description: "Browse, manage, and install extensions from the extension gallery", 
                  guid: new Guid("{27B24F4A-E2FD-42D0-8B9F-639E99E72A35}"), 
                  isSystemExtension: true)
        {
        }

        public override void Enable()
        {
            ViewGalleryCommand = new VisualDelegateCommand(ViewGallery, "Extensions Manager", 
                ImageFactory.GetImage("irAuthor", "Images/Extension32.png"), 
                ImageFactory.GetImage("irAuthor", "Images/Extension32.png"), 
                true);

            var homeTab = IrAuthorShell.HomeTab;
            var group = homeTab.AddGroup("Extension Gallery", ImageFactory.GetImage("irAuthor", "Images/SmileFace16.png"));
            group.AddButton(ViewGalleryCommand);


        }

        private void ViewGallery(object obj)
        {
            var window = new ExtensionBrowser();
            window.Owner = Application.Current.MainWindow;
            window.Show();
        }
    }
}
