using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [UsedImplicitly]
    public class IpModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder.AddSingleton<AllowedIpAddressHandler>();
        }
    }
}
