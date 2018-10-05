using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.IisHost.Areas.AutoDeploy;
using Milou.Deployer.Web.IisHost.Areas.Configuration.Modules;
using Milou.Deployer.Web.IisHost.Areas.NuGet;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [RegistrationOrder(int.MaxValue, Tag = Scope.AspNetCoreScope)]
    [UsedImplicitly]
    public class BackgroundServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RefreshCacheBackgroundService>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<ServerAddressesFeature>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ConfigurationBackgroundService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<AutoDeployBackgroundService>().AsImplementedInterfaces().SingleInstance();
        }
    }
}