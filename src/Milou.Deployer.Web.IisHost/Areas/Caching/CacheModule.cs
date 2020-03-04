using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Caching;

namespace Milou.Deployer.Web.IisHost.Areas.Caching
{
    [UsedImplicitly]
    public class CacheModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder) =>
            builder
                .AddSingleton<IMemoryCache, MemoryCache>(new MemoryCache(new MemoryCacheOptions()), this)
                .AddSingleton<ICustomMemoryCache, CustomMemoryCache>(this);
    }
}