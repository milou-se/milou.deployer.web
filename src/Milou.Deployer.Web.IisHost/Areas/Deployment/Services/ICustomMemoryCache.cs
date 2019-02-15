using System.Collections.Generic;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public interface ICustomMemoryCache
    {
        IReadOnlyCollection<string> CachedKeys { get; }

        bool TryGetValue<T>([NotNull] string key, out T item) where T : class;

        void Set<T>([NotNull] string key, [NotNull] T item) where T : class;

        void Invalidate(string prefix = null);
    }
}