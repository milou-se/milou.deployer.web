using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Caching
{
    public static class InMemoryCache
    {
        private static readonly ConcurrentDictionary<string, PackageVersion> _PackageVersions =
            new ConcurrentDictionary<string, PackageVersion>();

        public static IReadOnlyCollection<PackageVersion> All => _PackageVersions.Values.SafeToReadOnlyCollection();

        public static void Add(PackageVersion packageVersion)
        {
            if (packageVersion == null)
            {
                throw new ArgumentNullException(nameof(packageVersion));
            }

            _PackageVersions.TryAdd(packageVersion.Key, packageVersion);
        }

        public static void Invalidate()
        {
            _PackageVersions.Clear();
        }
    }
}