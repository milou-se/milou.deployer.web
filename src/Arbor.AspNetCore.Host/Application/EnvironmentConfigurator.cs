using System;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Application
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
