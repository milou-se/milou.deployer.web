using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    public class ServiceModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            builder.AddSingleton<PackageService>();
            builder.AddSingleton<DeploymentService>();
            builder.AddSingleton<MilouDeployer>();
            builder.AddSingleton(
                context => new MilouDeployerConfiguration(context.GetService<IKeyValueConfiguration>()));

            builder.AddSingleton<ICredentialReadService, ConfigurationCredentialReadService>();
            return builder;
        }
    }
}
