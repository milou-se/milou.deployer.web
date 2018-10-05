using Autofac;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core
{
    [UsedImplicitly]
    public class TimeModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CustomSystemClock>()
                .AsImplementedInterfaces()
                .SingleInstance()
                .PreserveExistingDefaults();
        }
    }
}