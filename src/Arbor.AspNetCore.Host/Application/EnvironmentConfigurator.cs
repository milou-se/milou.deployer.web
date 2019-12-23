using System;
using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Arbor.AspNetCore.Host.Application
{
    public static class EnvironmentConfigurator
    {
        public static void ConfigureEnvironment([NotNull] ConfigurationInstanceHolder configurationInstanceHolder)
        {
            if (configurationInstanceHolder == null)
            {
                throw new ArgumentNullException(nameof(configurationInstanceHolder));
            }

            var configureEnvironments = configurationInstanceHolder.CreateInstances<IConfigureEnvironment>();
            var environmentConfiguration = configurationInstanceHolder.Get<EnvironmentConfiguration>();

            foreach (var configureEnvironment in configureEnvironments)
            {
                configureEnvironment.Configure(environmentConfiguration);
            }
        }
    }
}
