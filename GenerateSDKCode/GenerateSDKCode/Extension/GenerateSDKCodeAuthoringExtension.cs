using System;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Extension
{
    
   
    public class GenerateSDKCodeAuthoringExtension : ExtensionBase
    {
        private const string ExtensionGUID = "{903C7018-213F-4A32-B3F2-0B6DA295E02E}";
        public new ServiceManager ServiceManager => base.ServiceManager;
        private ICommandProvider _commandProvider;
        public GenerateSDKCodeAuthoringExtension()
            : base("Generate SDK Code", "Generate SDK Code to programmatically author elements using irSDK via the right-click menu.", new Guid(ExtensionGUID), false)
        {
            
        }
        public bool IsEnabled = false;
        public override void Enable()
        {
            if(!IsEnabled)
            {
                _commandProvider = new GenerateSDKCodeCommandProvider(ServiceManager);
                ServiceManager.GetService<CommandService>().AddProvider(_commandProvider);
                IsEnabled = true;
            }
        }

        public override void Disable()
        {
            if (IsEnabled)
            {
                CommandService.RemoveProvider(_commandProvider);
                IsEnabled = false;
            }
        }
    }

    

    
}