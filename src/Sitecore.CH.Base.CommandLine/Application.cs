using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Sitecore.CH.Base.Dependencies;
using Sitecore.CH.Base.CommandLine.Dependencies;

namespace Sitecore.CH.Base.CommandLine
{
    public class Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public void Startup()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            AddServices(serviceCollection);

            var builder = new ContainerBuilder();

            //A bit ovekill to load all dlls that we have, can be improved later.
            string[] assemblyScanerPattern = new[] { @".*\.dll$" };

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var currentDomain = AppDomain.CurrentDomain;

            List<Assembly> assemblies = new List<Assembly>();
            foreach (var assemblyFileName in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.dll", SearchOption.AllDirectories)
                         .Where(filename => assemblyScanerPattern.Any(pattern => Regex.IsMatch(filename, pattern))))
            {
                try
                {
                    assemblies.Add(Assembly.LoadFrom(assemblyFileName));
                }
                catch (FileLoadException) { }
            }

            builder.RegisterAssemblyModules(assemblies.ToArray());

            builder.Populate(serviceCollection);
            var container = builder.Build();
            ServiceProvider = new AutofacServiceProvider(container);
        }

        protected virtual void AddServices(IServiceCollection serviceCollection)
        {

            var configBuilder = new ConfigurationBuilder();

            var config = GetConfig(configBuilder);

            serviceCollection.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddNLog("nlog.config");
            });

            serviceCollection.AddMClient((options) => { config.Bind(Base.Constants.Config.Sections.MSection, options); });
            serviceCollection.AddScriptConfig((options) => { config.Bind(Constants.Config.Sections.ScriptPushSection, options); });

            serviceCollection.AddBaseServices();
        }

        protected IConfiguration GetConfig(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder = configurationBuilder.SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

            configurationBuilder = GetOverrides(configurationBuilder);

            return configurationBuilder.Build();
        }

        protected virtual IConfigurationBuilder GetOverrides(IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder;
        }
    }
}
