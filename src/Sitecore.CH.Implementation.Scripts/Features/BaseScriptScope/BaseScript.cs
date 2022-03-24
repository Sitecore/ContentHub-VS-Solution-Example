using Stylelabs.M.Scripting.Types.V1_0.Action;
using Stylelabs.M.Sdk;
using System.Threading.Tasks;

namespace Sitecore.CH.Implementation.Scripts.Features.BaseScriptScope
{
    public abstract class BaseScript
    {
        protected IMClient MClient;

        public BaseScript(IMClient client)
        {
            MClient = client;
        }
        public abstract Task Run();
    }
}
