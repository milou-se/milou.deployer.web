using Arbor.KVConfiguration.Core;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.NuGet;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{

    [UsedImplicitly]
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DeploymentService>().AsSelf().AsImplementedInterfaces();
            builder.RegisterType<MilouDeployer>().AsSelf();
            builder.Register(context => new MilouDeployerConfiguration(context.Resolve<IKeyValueConfiguration>())).SingleInstance();
            builder.RegisterType<ConfigurationCredentialReadService>().AsImplementedInterfaces().SingleInstance();
        }
    }
}