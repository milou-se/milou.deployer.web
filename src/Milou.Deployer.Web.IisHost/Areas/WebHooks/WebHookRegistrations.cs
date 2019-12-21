using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.DependencyInjection;

namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    [UsedImplicitly]
    public class WebHookRegistrations : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder.AddSingleton<PackageWebHookHandler>()
                .RegisterAssemblyTypes<IPackageWebHook>(ApplicationAssemblies.FilteredAssemblies(), ServiceLifetime.Singleton);
        }
    }
}