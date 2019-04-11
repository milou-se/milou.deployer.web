using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;

namespace Milou.Deployer.Web.Core.Deployment
{
    public static class PackageVersionExtensions
    {
        public static SemanticVersion Latest(this IEnumerable<PackageVersion> packageVersions)
        {
            return packageVersions.OrderByDescending(package => package.Version).FirstOrDefault()?.Version;
        }
    }
}
