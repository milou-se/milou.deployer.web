using Autofac;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [UsedImplicitly]
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<App>()
                .AsSelf();
        }
    }
}
