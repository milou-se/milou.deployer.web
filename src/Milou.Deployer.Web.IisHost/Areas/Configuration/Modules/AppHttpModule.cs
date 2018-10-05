using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Http;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    //[RegistrationOrder(0, Tag = Scope.AspNetCoreScope, ReRegisterEnabled = true)]
    public class AppHttpModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CustomHttpClientFactory>().AsSelf();
        }
    }
}