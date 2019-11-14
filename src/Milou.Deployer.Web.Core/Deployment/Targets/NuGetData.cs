using System;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class NuGetData
    {
        public TimeSpan? PackageListTimeout { get; set; }

        public string NuGetConfigFile { get; set; }

        public string NuGetPackageSource { get; set; }
    }
}