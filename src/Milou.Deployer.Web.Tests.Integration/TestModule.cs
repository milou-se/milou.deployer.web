using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Targets;
using Serilog;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class TestModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder.AddSingleton<IDeploymentTargetReadService>(context =>
                new InMemoryDeploymentTargetReadService(context.GetService<ILogger>(), TestDataCreator.CreateData));
        }
    }
}
