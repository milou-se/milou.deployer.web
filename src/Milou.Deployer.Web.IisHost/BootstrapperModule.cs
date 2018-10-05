using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost
{
    [UsedImplicitly]
    public class BootstrapperModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BootstrapperClient>().SingleInstance();
        }
    }
}