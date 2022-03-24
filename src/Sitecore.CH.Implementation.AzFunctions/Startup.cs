using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.CH.Base.Dependencies;
using Sitecore.CH.Implementation.AzFunctions.Dependencies;
using Sitecore.CH.Implementation.Dependencies;
using System;

[assembly: FunctionsStartup(typeof(Sitecore.CH.Implementation.AzFunctions.Startup))]

namespace Sitecore.CH.Implementation.AzFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)                
                .AddUserSecrets<Startup>()
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);

            builder.Services.AddLogging();

            builder.Services.AddMClient((options) => { config.Bind(Base.Constants.Config.Sections.MSection, options); });

            builder.Services.AddBaseServices();

            builder.Services.AddImplementationServices();

            builder.Services.AddAzFunctionServices();


        }
    }
}
