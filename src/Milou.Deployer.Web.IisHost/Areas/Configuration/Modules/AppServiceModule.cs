using System;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Targets;
using Milou.Deployer.Web.IisHost.Areas.Secrets;
using Milou.Deployer.Web.IisHost.Areas.Targets;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    public class AppServiceModule : Module
    {
        private readonly IKeyValueConfiguration _keyValueConfiguration;
        private readonly ILogger _logger;

        public AppServiceModule([NotNull] IKeyValueConfiguration keyValueConfiguration, [NotNull] ILogger logger)
        {
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override void Load(ContainerBuilder builder)
        {
            string jsonSourceValue =
                _keyValueConfiguration[ConfigurationConstants.JsonSourceEnabled];

            JsonDeploymentTargetSourceConfiguration sourceConfiguration =
                _keyValueConfiguration.GetInstance<JsonDeploymentTargetSourceConfiguration>() ??
                new JsonDeploymentTargetSourceConfiguration("");

            if (sourceConfiguration.SourceFile.HasValue() &&
                (!bool.TryParse(jsonSourceValue, out bool jsonSource) || jsonSource))
            {
                _logger.Information("Using JSON as primary target source");

                builder.RegisterType<JsonTargetSource>()
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .SingleInstance()
                    .PreserveExistingDefaults();

                builder.RegisterType<JsonDeploymentTargetReadService>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
                    .PreserveExistingDefaults();

                builder.RegisterType<ConfigurationCredentialReadService>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
                    .PreserveExistingDefaults();

                builder.RegisterInstance(sourceConfiguration)
                    .AsSelf()
                    .SingleInstance()
                    .PreserveExistingDefaults();
            }
            else
            {
                _logger.Information("Using in-memory data as primary target source");
                builder.RegisterType<InMemoryDeploymentTargetReadService>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
                    .PreserveExistingDefaults();

                builder.RegisterType<InMemoryCredentialReadService>()
                    .AsImplementedInterfaces()
                    .SingleInstance()
                    .PreserveExistingDefaults();
            }
        }
    }
}