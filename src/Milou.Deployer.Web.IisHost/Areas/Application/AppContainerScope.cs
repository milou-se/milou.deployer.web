using System;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public class AppContainerScope
    {
        public AppContainerScope([NotNull] IContainer container, [NotNull] Scope rootScope)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            AppRootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
        }

        public IContainer Container { get; }

        public Scope AppRootScope { get; }
    }
}