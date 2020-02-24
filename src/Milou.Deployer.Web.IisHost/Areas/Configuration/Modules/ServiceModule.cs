using Arbor.App.Extensions.DependencyInjection;
using Arbor.KVConfiguration.Core;
using Arbor.Tooler;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Milou.Deployer.Web.Agent;
using Milou.Deployer.Web.Core.Credentials;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.NuGet;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    public class ServiceModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            builder.AddSingleton<PackageService>(this);
            builder.Add<IDeploymentService, DeploymentService>(ServiceLifetime.Transient, this);
            builder.AddSingleton(
                context => new MilouDeployerConfiguration(context.GetService<IKeyValueConfiguration>()),
                this);

            builder.AddSingleton<ICredentialReadService, ConfigurationCredentialReadService>(this);
            builder.AddSingleton<NuGetPackageInstaller>();

            return builder;
        }
    }
}
