using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Caching;
using Milou.Deployer.Web.Core.DependencyInjection;

namespace Milou.Deployer.Web.IisHost.Areas.Caching
{
    [UsedImplicitly]
    public class CacheModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder
                .AddSingleton<IMemoryCache, MemoryCache>(new MemoryCache(new MemoryCacheOptions()), this)
                .AddSingleton<ICustomMemoryCache, CustomMemoryCache>(this);
        }
    }
}
