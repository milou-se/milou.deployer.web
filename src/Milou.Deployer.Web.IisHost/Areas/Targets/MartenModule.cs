using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Autofac;
using JetBrains.Annotations;
using Marten;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Targets;

namespace Milou.Deployer.Web.IisHost.Areas.Targets
{
    [Order(int.MaxValue)]
    [UsedImplicitly]
    public class MartenModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var configuration = StaticKeyValueConfigurationManager.AppSettings.GetInstance<MartenConfiguration>();

            if (configuration is null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(configuration.ConnectionString) && configuration.Enabled)
            {
                builder.RegisterType<MartenTargetReadService>()
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
        }
    }
}