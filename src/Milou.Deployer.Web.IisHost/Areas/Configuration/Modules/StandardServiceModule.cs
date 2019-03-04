using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    [RegistrationOrder(0, Tag = Scope.AspNetCoreScope)]
    public class StandardServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MonitoringService>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
