using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;

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

            foreach (IConfigureEnvironment configureEnvironment in configureEnvironments)
            {
                configureEnvironment.Configure(environmentConfiguration);
            }
        }
    }
}