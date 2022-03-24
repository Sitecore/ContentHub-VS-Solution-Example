using Microsoft.Extensions.DependencyInjection;
using Sitecore.CH.Implementation.Features.Sample.Services;
using System;


namespace Sitecore.CH.Implementation.Dependencies
{
    public static class ServicesRegistrationCollectionExtensions
    {
        public static IServiceCollection AddImplementationServices(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            collection.AddTransient<ISampleImplementationService, SampleImplementationService>();

            return collection;
        }
    }
}
