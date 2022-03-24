using Stylelabs.M.Scripting.Types.V1_0.Processing.Metadata;
using Stylelabs.M.Sdk;

namespace Sitecore.CH.Implementation.Scripts.Features.BaseScriptScope
{
    public abstract class BaseMetadataScriptScope : BaseScript
    {
        protected IMetadataProcessingContext Context { get; }
        public BaseMetadataScriptScope(IMetadataProcessingContext context, IMClient client) : base(client)
        {
            Context = context;
        }
    }
}
