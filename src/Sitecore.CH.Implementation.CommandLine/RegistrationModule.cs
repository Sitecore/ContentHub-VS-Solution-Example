using Autofac;
using ManyConsole;

namespace Sitecore.CH.Implementation.CommandLine
{
    public class RegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(RegistrationModule).Assembly);
            builder.RegisterAssemblyTypes(typeof(RegistrationModule).Assembly).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(RegistrationModule).Assembly).Where(t => t.IsAssignableTo<ConsoleCommand>()).As<ConsoleCommand>();
            base.Load(builder);
        }
    }
}
