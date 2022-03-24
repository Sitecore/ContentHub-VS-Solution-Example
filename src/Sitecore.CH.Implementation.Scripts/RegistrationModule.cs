using Autofac;

namespace Sitecore.CH.Implementation.Scripts
{
    public class RegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(RegistrationModule).Assembly);
            builder.RegisterAssemblyTypes(typeof(RegistrationModule).Assembly).AsImplementedInterfaces();
            base.Load(builder);
        }
    }
}
