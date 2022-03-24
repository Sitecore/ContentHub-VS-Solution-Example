using Stylelabs.M.Scripting.Types.V1_0.User.SignIn;
using Stylelabs.M.Sdk;

namespace Sitecore.CH.Implementation.Scripts.Features.BaseScriptScope
{
    public abstract class BaseUserSignInScriptScope : BaseScript
    {
        protected IUserSignInContext Context;

        public BaseUserSignInScriptScope(IUserSignInContext context, IMClient client) : base(client)
        {
            Context = context;
        }
    }
}
