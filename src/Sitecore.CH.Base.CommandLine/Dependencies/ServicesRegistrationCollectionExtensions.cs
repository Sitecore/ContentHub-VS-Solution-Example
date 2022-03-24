using Microsoft.Extensions.DependencyInjection;
using Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Config;
using System;

namespace Sitecore.CH.Base.CommandLine.Dependencies
{
    public static class ServicesRegistrationCollectionExtensions
    {
        public static IServiceCollection AddScriptConfig(this IServiceCollection collection,
        Action<ScriptPushSettings> setupAction)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            collection.Configure(setupAction);
            return collection;
        }
    }
}
