using Microsoft.Extensions.DependencyInjection;
using Sitecore.CH.Base.Features.Base.Services;
using Sitecore.CH.Base.Features.CRUD.Services;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.Sample.Services;
using Sitecore.CH.Base.Features.Security.Services;
using System;

namespace Sitecore.CH.Base.Dependencies
{
    public static class ServicesRegistrationCollectionExtensions
    {
        public static IServiceCollection AddBaseServices(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            //Here we can add registrations specific to Sitecore.CH.Base

            collection.AddTransient<ISampleBaseService, SampleBaseService>();
            collection.AddTransient<IBaseEntityService, BaseEntityService>();
            collection.AddTransient(typeof(ILoggerService<>), typeof(LoggerService<>));
            collection.AddScoped<ILoggingContextService, LoggingContextService>();
            collection.AddTransient<IMassDeleteJobService, MassDeleteJobService>();
            collection.AddTransient<IBaseJobService, BaseJobService>();
            collection.AddScoped<ISecurityService, SecurityService>();
            return collection;
        }
    }
}
