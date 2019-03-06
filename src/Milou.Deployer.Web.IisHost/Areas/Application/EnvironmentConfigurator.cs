using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class EnvironmentConfigurator
    {
        public static void ConfigureEnvironment([NotNull] ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null)
            {
                throw new ArgumentNullException(nameof(lifetimeScope));
            }

            var configureEnvironments = lifetimeScope.Resolve<IReadOnlyCollection<IConfigureEnvironment>>();
            var environmentConfiguration = lifetimeScope.Resolve<EnvironmentConfiguration>();

            foreach (var configureEnvironment in configureEnvironments)
            {
                configureEnvironment.Configure(environmentConfiguration);
            }
        }
    }
}
