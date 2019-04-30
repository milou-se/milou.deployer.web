using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [UsedImplicitly]
    public class StartupModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder.AddSingleton(context => new StartupTaskContext(context.GetServices<
                IStartupTask>()));
        }
    }
}
