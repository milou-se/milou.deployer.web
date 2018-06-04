using System;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public sealed class Scope : IDisposable
    {
        public const string AspNetCoreScope = "ASPNETCORE";

        public Scope([NotNull] ILifetimeScope lifeTimeScope, Scope subScope = null)
        {
            Lifetime = lifeTimeScope ?? throw new ArgumentNullException(nameof(lifeTimeScope));
            SubScope = subScope;
        }

        public Scope Deepest()
        {
            if (SubScope.HasValue())
            {
                return SubScope.Deepest();
            }

            return this;
        }

        public ILifetimeScope Lifetime { get; }

        public Scope SubScope { get; set; }

        public void Dispose()
        {
            if (SubScope.HasValue())
            {
                SubScope.Dispose();
            }

            Lifetime?.Dispose();
        }
    }
}