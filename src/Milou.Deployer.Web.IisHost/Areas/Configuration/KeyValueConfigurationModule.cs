using System;
using Arbor.KVConfiguration.Core;
using Autofac;
using JetBrains.Annotations;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration
{
    public class KeyValueConfigurationModule : Module
    {
        private readonly MultiSourceKeyValueConfiguration _multiSourceKeyValueConfiguration;
        private readonly ILogger _logger;

        public KeyValueConfigurationModule([NotNull] MultiSourceKeyValueConfiguration configuration, ILogger logger)
        {
            _multiSourceKeyValueConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        protected override void Load(ContainerBuilder builder)
        {
            _logger.Debug("Registering configuration");

            builder.RegisterInstance(_multiSourceKeyValueConfiguration)
                .AsSelf()
                .AsImplementedInterfaces().SingleInstance();
        }
    }
}
