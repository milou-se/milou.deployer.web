using System;
using Autofac;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public class AppContainerScope
    {
        public AppContainerScope([NotNull] IContainer container, [NotNull] ILifetimeScope appRootScope)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            AppRootScope = appRootScope ?? throw new ArgumentNullException(nameof(appRootScope));
        }

        public IContainer Container { get; }

        public ILifetimeScope AppRootScope { get; }
    }
}