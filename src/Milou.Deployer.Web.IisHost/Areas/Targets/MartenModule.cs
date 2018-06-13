using System;
using System.Collections.Immutable;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Autofac;
using JetBrains.Annotations;
using Marten;
using Marten.Services;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Json;
using Milou.Deployer.Web.Core.Targets;

namespace Milou.Deployer.Web.IisHost.Areas.Targets
{
    [RegistrationOrder(1000)]
    [UsedImplicitly]
    public class MartenModule : Module
    {
        private readonly IKeyValueConfiguration _keyValueConfiguration;

        public MartenModule(IKeyValueConfiguration keyValueConfiguration)
        {
            _keyValueConfiguration = keyValueConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            ImmutableArray<MartenConfiguration> configurations = _keyValueConfiguration.GetInstances<MartenConfiguration>();

            if (configurations.IsDefaultOrEmpty)
            {
                return;
            }

            if (configurations.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly 1 instance of type {nameof(MartenConfiguration)} but got {configurations.Length}");
            }

            MartenConfiguration configuration = configurations.Single();

            if (!string.IsNullOrWhiteSpace(configuration.ConnectionString) && configuration.Enabled)
            {
                builder.RegisterType<MartenStore>()
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.Register(context =>
                        DocumentStore.For(options => ConfigureMarten(options, configuration.ConnectionString)))
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .SingleInstance();
            }
        }

        private void ConfigureMarten(StoreOptions options, string connectionString)
        {
            options.Connection(connectionString);

            var jsonNetSerializer = new JsonNetSerializer();

            jsonNetSerializer.Customize(serializer => serializer.UseCustomConverters());

            options.Serializer(jsonNetSerializer);
        }
    }
}