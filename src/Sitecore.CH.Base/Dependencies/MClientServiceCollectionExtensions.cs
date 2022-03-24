using Microsoft.Extensions.DependencyInjection;
using Sitecore.CH.Base.Features.SDK.Services;
using Sitecore.CH.Base.Features.SDK.Services.Config;
using System;

namespace Sitecore.CH.Base.Dependencies
{
    public static class MClientServiceCollectionExtensions
    {
        public static IServiceCollection AddMClient(this IServiceCollection collection,
        Action<MClientOptions> setupAction)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            collection.Configure(setupAction);
            return collection.AddSingleton<IMClientFactory, MClientFactory>();
        }
    }
}
