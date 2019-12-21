using System;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class TargetNuGetSettings
    {
        public string NuGetConfigFile { get; set; }

        public string NuGetPackageSource { get; set; }

        public TimeSpan? PackageListTimeout { get; set; }
    }
}