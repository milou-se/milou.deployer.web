using System;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Autofac;
using JetBrains.Annotations;
using Marten;
using Marten.Services;
using MediatR;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Json;

namespace Milou.Deployer.Web.Marten
{
    [RegistrationOrder(1000)]
    [UsedImplicitly]
    public class MartenModule : Module
    {
        private readonly IKeyValueConfiguration _keyValueConfiguration;

        public MartenModule([NotNull] IKeyValueConfiguration keyValueConfiguration)
        {
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
        }

        private void ConfigureMarten(StoreOptions options, string connectionString)
        {
            options.Connection(connectionString);

            var jsonNetSerializer = new JsonNetSerializer();

            jsonNetSerializer.Customize(serializer => serializer.UseCustomConverters());

            options.Serializer(jsonNetSerializer);
        }

        protected override void Load(ContainerBuilder builder)
        {
            var configurations = _keyValueConfiguration.GetInstances<MartenConfiguration>();

            if (configurations.IsDefaultOrEmpty)
            {
                return;
            }

            if (configurations.Length > 1)
            {
                builder.RegisterInstance(new MartenConfiguration(string.Empty));
                builder.RegisterInstance(new ConfigurationError(
                    $"Expected exactly 1 instance of type {nameof(MartenConfiguration)} but got {configurations.Length}"));
                return;
            }

            var configuration = configurations.Single();

            if (!string.IsNullOrWhiteSpace(configuration.ConnectionString) && configuration.Enabled)
            {
                builder.RegisterAssemblyTypes(typeof(MartenStore).Assembly)
                    .Where(type => type == typeof(MartenStore))
                    .AsClosedTypesOf(typeof(IRequestHandler<,>))
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.Register(context =>
                        DocumentStore.For(options => ConfigureMarten(options, configuration.ConnectionString)))
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .SingleInstance();
            }
        }
    }
}
