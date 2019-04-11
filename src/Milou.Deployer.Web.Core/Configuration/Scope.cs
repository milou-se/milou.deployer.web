using System;
using Autofac;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Configuration
{
    public sealed class Scope : IDisposable
    {
        public const string WebHostScope = "WebHostScope";

        public const string AppRootScopeName = "AppScope";

        public const string RootScopeName = "RootScope";

        public const string AspNetCoreScope = "ASPNETCORE";

        private Scope _subScope;

        public Scope(
            [NotNull] string name,
            [CanBeNull] ILifetimeScope lifeTimeScope = null,
            [CanBeNull] Scope subScope = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("message", nameof(name));
            }

            Name = name;
            Lifetime = lifeTimeScope;
            SubScope = subScope;
        }

        public string Name { get; }

        public ILifetimeScope Lifetime { get; set; }

        [PublicAPI]
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

        public Scope Parent { get; private set; }

        public override string ToString()
        {
            return $"ScopeName: {Name}";
        }

        public string Diagnostics()
        {
            var subScope = SubScope is null ? null : " --> " + SubScope.Diagnostics();
            return Name + subScope;
        }

        public Scope Deepest()
        {
            if (SubScope.HasValue())
            {
                return SubScope.Deepest();
            }

            return this;
        }

        public Scope Top()
        {
            var current = this;

            while (current.Parent != null)
            {
                current = current.Parent;
            }

            return current;
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
