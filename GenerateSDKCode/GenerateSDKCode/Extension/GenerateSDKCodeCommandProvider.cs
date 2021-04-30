using System.Collections.Generic;
using InRule.Authoring.Commanding;
using InRule.Authoring.Services;
using InRule.Repository;
using InRuleLabs.AuthoringExtensions.GenerateSDKCode.Features.GenerateSDKCode;

namespace InRuleLabs.AuthoringExtensions.GenerateSDKCode.Extension
{
    public class GenerateSDKCodeCommandProvider : ICommandProvider
    {
        private readonly ServiceManager _serviceManager;
        private readonly RuleApplicationService _ruleApplicationService;

        public GenerateSDKCodeCommandProvider(ServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
            _ruleApplicationService = serviceManager.GetService<RuleApplicationService>();
        }

        public IEnumerable<IVisualCommand> GetCommands(object o)
        {
            var def = o as RuleRepositoryDefBase;
            var commands = new List<IVisualCommand>();
            var controller = _ruleApplicationService.Controller;
            if (def != null)
            {
                commands.Add(_serviceManager.Compose<ShowSDKCodeCommand>(def));
                commands.Add(_serviceManager.Compose<ShowSDKCodeCommand>(def.GetRuleApp()));
            }

            return commands;
        }
    }
}
