using System;
using System.Windows;
using System.Collections.Generic;
using ExtensionManager.ViewModels;
using ExtensionManager.Views;
using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using System.Linq;

namespace ExtensionManager
{
    public class Extension : ExtensionBase
    {
        public VisualDelegateCommand ViewGalleryCommand { get; private set; }
        
        public Extension() 
            : base(name: "Extension Manager for IrAuthor", 
                  description: "Browse, manage, and install extensions from the extension gallery", 
                  guid: new Guid("{A948BB74-0A66-4C71-858D-0225C0D17AAB}"), 
                  isSystemExtension: true)
        {
            Priority = 100000000; //ensures that the extension loads after other system extensions so that it can be the furthest element to the right.
        }

        public override void Enable()
        {
            ViewGalleryCommand = new VisualDelegateCommand(ViewGallery, "Extensions Manager", 
                ImageFactory.GetImage("irAuthor", "Images/Extension32.png"), 
                ImageFactory.GetImage("irAuthor", "Images/Extension32.png"), 
                true);
            
            var homeTab = IrAuthorShell.HomeTab;
            var group = homeTab.AddGroup("Extension Gallery", ImageFactory.GetImage("irAuthor", "Images/SmileFace16.png"));
            var button = group.AddButton(ViewGalleryCommand);

        }

        private void ViewGallery(object obj)
        {
            var settings = SettingsStorageService.LoadSettings<ExtensionManagerSettings>(new Guid(Constants.SettingsKey), true);
            
            var viewModel = new ExtensionBrowserViewModel(settings);
            viewModel.SettingsChanged +=
                (sender, managerSettings) =>
                    SettingsStorageService.SaveSettings(new Guid(Constants.SettingsKey), "", managerSettings);

            var window = new ExtensionBrowser(viewModel)
            {
                Owner = Application.Current.MainWindow
            };
            window.Show();
        }
    }
}
