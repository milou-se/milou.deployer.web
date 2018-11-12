using Autofac;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Time
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