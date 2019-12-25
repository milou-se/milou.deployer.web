using System.Linq;
using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.NuGet;

namespace Milou.Deployer.Web.IisHost.Areas.NuGet
{
    [UsedImplicitly]
    public class NuGetModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            if (!builder.Any(s => s.ImplementationType == typeof(NuGetCacheConfiguration)))
            {
                builder.Add(typeof(NuGetConfiguration), ServiceLifetime.Singleton, this);
            }

            return builder;
        }
    }
}