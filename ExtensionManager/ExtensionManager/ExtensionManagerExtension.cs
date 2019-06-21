using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using ExtensionManager.ViewModels;
using ExtensionManager.Views;
using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Settings;

namespace ExtensionManager
{
    public sealed class ExtensionManagerExtension : ExtensionBase
    {
        [ImportMany]
        private IEnumerable<IExtension> extensions; 

        public VisualDelegateCommand ViewGalleryCommand { get; private set; }
        
        public ExtensionManagerExtension() 
            : base(name: "Extension Manager for IrAuthor", 
                  description: "Browse, manage, and install extensions from the extension gallery", 
                  guid: new Guid("{A948BB74-0A66-4C71-858D-0225C0D17AAB}"), 
                  isSystemExtension: true)
        {
            Priority = 100000000; // HACK: ensures that the extension loads after other system extensions so that it can be the furthest element to the right.
        }

        public override void Enable()
        {
            ViewGalleryCommand = new VisualDelegateCommand(ViewGallery, "Extensions Manager", 
                ImageFactory.GetImage("irAuthor", "Images/Extension32.png"), 
                ImageFactory.GetImage("irAuthor", "Images/Extension32.png"), 
                true);
            
            var homeTab = IrAuthorShell.HomeTab;
            var group = homeTab.AddGroup("Extension Gallery", ImageFactory.GetImage("irAuthor", "Images/SmileFace16.png"));
            _ = group.AddButton(ViewGalleryCommand);
        }

        private void ViewGallery(object obj)
        {
            var settings = SettingsStorageService.LoadSettings<ExtensionManagerSettings>(new Guid(Constants.SettingsKey), true);
            var irAuthorSettings = SettingsStorageService.LoadSettings<IrAuthorSettings>(ExtensionManager.Constants.IrAuthorSettings);
            settings.EnabledExtensions = irAuthorSettings.EnabledUserExtensions;

            var viewModel = ServiceManager.Compose<ExtensionBrowserViewModel>(settings);
            viewModel.InstalledExtensions = extensions;
            
            viewModel.SettingsChanged +=
                (sender, managerSettings) =>
                    SettingsStorageService.SaveSettings(new Guid(Constants.SettingsKey), "", managerSettings);

            var window = new ExtensionBrowser(viewModel)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }
    }
}
