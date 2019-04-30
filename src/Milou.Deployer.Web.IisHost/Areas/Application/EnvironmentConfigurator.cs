using System;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class EnvironmentConfigurator
    {
        public static void ConfigureEnvironment([NotNull] ConfigurationInstanceHolder lifetimeScope)
        {
            if (lifetimeScope == null)
            {
                throw new ArgumentNullException(nameof(lifetimeScope));
            }

            var configureEnvironments = lifetimeScope.GetInstances<IConfigureEnvironment>().Values;
            var environmentConfiguration = lifetimeScope.Get<EnvironmentConfiguration>();

            foreach (var configureEnvironment in configureEnvironments)
            {
                configureEnvironment.Configure(environmentConfiguration);
            }
        }
    }
}
