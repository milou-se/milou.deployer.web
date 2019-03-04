using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [UsedImplicitly]
    public class CacheModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new MemoryCache(new MemoryCacheOptions()))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<CustomMemoryCache>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
