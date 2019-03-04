using Arbor.KVConfiguration.Core;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    [RegistrationOrder(0, Tag = Scope.AspNetCoreScope, RegisterInRootScope = true)]
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PackageService>().AsSelf().AsImplementedInterfaces();
            builder.RegisterType<DeploymentService>().AsSelf().AsImplementedInterfaces();
            builder.RegisterType<MilouDeployer>().AsSelf();
            builder.Register(context => new MilouDeployerConfiguration(context.Resolve<IKeyValueConfiguration>()))
                .SingleInstance();
            builder.RegisterType<ConfigurationCredentialReadService>().AsImplementedInterfaces().SingleInstance();
        }
    }
}
