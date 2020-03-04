using Arbor.App.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Startup;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Startup
{
    [UsedImplicitly]
    public class StartupModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder) =>
            builder.AddSingleton(context => new StartupTaskContext(context.GetServices<IStartupTask>()), this);
    }
}