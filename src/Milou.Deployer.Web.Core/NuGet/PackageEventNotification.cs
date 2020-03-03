using MediatR;
using Milou.Deployer.Web.Core.Deployment.Packages;

namespace Milou.Deployer.Web.Core.NuGet
{
    public class PackageEventNotification : INotification
    {
        public PackageEventNotification(PackageVersion packageVersion, string nugetSource, string nugetConfig)
        {
            PackageVersion = packageVersion;
            NugetSource = nugetSource;
            NugetConfig = nugetConfig;
        }

        public PackageVersion PackageVersion { get; }

        public string NugetSource { get; }

        public string NugetConfig { get; }
    }
}