using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;

namespace Milou.Deployer.Web.IisHost.Areas.Caching
{
    [UsedImplicitly]
    public class CacheModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder
                .AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()))
                .AddSingleton<ICustomMemoryCache, CustomMemoryCache>();
        }
    }
}
