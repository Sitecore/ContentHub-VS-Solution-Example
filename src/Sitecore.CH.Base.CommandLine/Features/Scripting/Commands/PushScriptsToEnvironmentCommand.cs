using ManyConsole;
using Nito.AsyncEx.Synchronous;
using Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Services;
using System;
using System.Threading.Tasks;

namespace Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Commands
{
    public class PushScriptsToEnvironmentCommand : ConsoleCommand
    {
        private string[] _scripts;
        private bool _createAction;
        private bool? _enableAssociatedTriggers;
        private bool? _disableAssociatedTriggers;
        private readonly ScriptService _scriptService;
        private const int Success = 0;

        public PushScriptsToEnvironmentCommand(ScriptService scriptService)
        {
            IsCommand("push-scripts-to-environment", "Creates or updates scripts in environment, compiles and publishes them");
            HasRequiredOption("scripts=", "Comma-separated list of scripts to publish", (string x) => _scripts = x.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
            HasOption("create-action", "Flag indicating whether related action should be created.", _ => _createAction = true);
            HasOption("enable-triggers", "Flag indicating whether related triggers should be enabled.", _ => _enableAssociatedTriggers = true);
            HasOption("disable-triggers", "Flag indicating whether related triggers should be disabled.", _ => _disableAssociatedTriggers = false);
            this._scriptService = scriptService;
        }


        public override int Run(string[] remainingArguments)
        {
            Validate();
            OnExecuteAsync().WaitAndUnwrapException();
            return Success;
        }

        private void Validate()
        {
            if (_enableAssociatedTriggers.HasValue && _disableAssociatedTriggers.HasValue)
                throw new InvalidOperationException("Please send only one enable or disable triggers option");
        }

        public async Task OnExecuteAsync()
        {
            await _scriptService.PushScripts(_scripts, _createAction, _enableAssociatedTriggers ?? _disableAssociatedTriggers).ConfigureAwait(false);
        }
    }
}
