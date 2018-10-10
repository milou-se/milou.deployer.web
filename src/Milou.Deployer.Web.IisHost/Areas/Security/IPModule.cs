using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [RegistrationOrder(0, Tag = Scope.AspNetCoreScope)]
    [UsedImplicitly]
    public class IPModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AllowedIPAddressHandler>().AsSelf().SingleInstance();
        }
    }
}