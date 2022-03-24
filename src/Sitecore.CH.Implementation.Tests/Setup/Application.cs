using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.CH.Implementation.AzFunctions.Dependencies;
using Sitecore.CH.Implementation.Dependencies;

namespace Sitecore.CH.Implementation.Tests
{
    public class Application : Sitecore.CH.Base.CommandLine.Application
    {

        protected override void AddServices(IServiceCollection serviceCollection)
        {
            
            base.AddServices(serviceCollection);
            serviceCollection.AddImplementationServices();
            serviceCollection.AddAzFunctionServices();
        }
        protected override IConfigurationBuilder GetOverrides(IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.AddUserSecrets<Application>();
        }

    }
}
