using System;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Configuration
{
    public sealed class Scope : IDisposable
    {
        public const string AspNetCoreScope = "ASPNETCORE";

        public Scope(ILifetimeScope lifeTimeScope = null, Scope subScope = null)
        {
            Lifetime = lifeTimeScope;
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

        public ILifetimeScope Lifetime { get; set; }

        [PublicAPI]
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