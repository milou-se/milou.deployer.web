using System;
using Autofac;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Configuration
{
    public sealed class Scope : IDisposable
    {
        private Scope _subScope;
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

        public Scope Parent { get; private set; }

        public Scope SubScope
        {
            get => _subScope;
            set
            {
                _subScope = value;

                if (_subScope != null)
                {
                    _subScope.Parent = this;
                }
            }
        }

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