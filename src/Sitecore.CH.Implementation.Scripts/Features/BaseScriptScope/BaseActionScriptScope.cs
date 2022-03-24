using Stylelabs.M.Scripting.Types.V1_0.Action;
using Stylelabs.M.Sdk;

namespace Sitecore.CH.Implementation.Scripts.Features.BaseScriptScope
{
    public abstract class BaseActionScriptScope : BaseScript
    {
        protected IActionScriptContext Context;

        public BaseActionScriptScope(IActionScriptContext context, IMClient client) : base(client)
        {
            Context = context;
        }
    }
}
