using Microsoft.Extensions.DependencyInjection;
using Sitecore.CH.Implementation.AzFunctions.Sample.Services;
using System;

namespace Sitecore.CH.Implementation.AzFunctions.Dependencies
{
    public static class ServicesRegistrationCollectionExtensions
    {
        public static IServiceCollection AddAzFunctionServices(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            collection.AddTransient<ISampleAzFunctionService, SampleAzFunctionService>();

            return collection;
        }
    }
}
