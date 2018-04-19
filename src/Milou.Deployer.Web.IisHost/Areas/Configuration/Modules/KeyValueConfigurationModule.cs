using System;
using Arbor.KVConfiguration.Core;
using Autofac;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    public class KeyValueConfigurationModule : Module
    {
        private readonly MultiSourceKeyValueConfiguration _multiSourceKeyValueConfiguration;

        public KeyValueConfigurationModule([NotNull] MultiSourceKeyValueConfiguration configuration)
        {
            _multiSourceKeyValueConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_multiSourceKeyValueConfiguration)
                .AsSelf()
                .AsImplementedInterfaces().SingleInstance();
        }
    }
}
