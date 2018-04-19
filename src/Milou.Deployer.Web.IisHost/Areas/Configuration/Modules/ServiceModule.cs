using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Targets;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.IisHost.Areas.NuGet;
using Milou.Deployer.Web.IisHost.Areas.Secrets;
using Milou.Deployer.Web.IisHost.Areas.Targets;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MonitoringService>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DeploymentService>().AsSelf().AsImplementedInterfaces();
            builder.RegisterType<NuGetService>().AsSelf().AsImplementedInterfaces();
            builder.RegisterType<MilouDeployer>().AsSelf();

            string jsonSourceValue =
                StaticKeyValueConfigurationManager.AppSettings[ConfigurationConstants.JsonSourceEnabled];

            JsonDeploymentTargetSourceConfiguration sourceConfiguration =
                StaticKeyValueConfigurationManager.AppSettings.GetInstance<JsonDeploymentTargetSourceConfiguration>() ??
                new JsonDeploymentTargetSourceConfiguration("");

            if (sourceConfiguration.SourceFile.HasValue() &&
                (!bool.TryParse(jsonSourceValue, out bool jsonSource) || jsonSource))
            {
                Log.Logger.Information("Using JSON as primary target source");

                builder.RegisterType<JsonTargetSource>().AsImplementedInterfaces().AsSelf().SingleInstance();
                builder.RegisterType<JsonDeploymentTargetReadService>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<ConfigurationCredentialReadService>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterInstance(sourceConfiguration).AsSelf().SingleInstance();
            }
            else
            {
                Log.Logger.Information("Using in-memory data as primary target source");
                builder.RegisterType<InMemoryDeploymentTargetReadService>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<InMemoryCredentialReadService>().AsImplementedInterfaces().SingleInstance();
            }

            builder.Register(context => new MilouDeployerConfiguration()).SingleInstance();
        }
    }
}