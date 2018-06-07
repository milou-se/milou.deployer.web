using System;
using System.Collections.Generic;
using MediatR;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class UpdateDeploymentTarget : IRequest<UpdateDeploymentTargetResult>
    {
        public UpdateDeploymentTarget(
            string id,
            bool allowExplicitPreRelease,
            Uri url,
            string packageId,
            string iisSiteName = null,
            string nugetPackageSource = null,
            string nugetConfigFile = null,
            bool autoDeployEnabled = false)
        {
            Id = id;
            AllowExplicitPreRelease = allowExplicitPreRelease;
            Url = url;
            PackageId = packageId;
            IisSiteName = iisSiteName;
            NugetPackageSource = nugetPackageSource;
            NugetConfigFile = nugetConfigFile;
            AutoDeployEnabled = autoDeployEnabled;
        }

        public string Id { get; }

        public Uri Url { get; }

        public bool AllowExplicitPreRelease { get; }

        public string IisSiteName { get; }

        public string NugetPackageSource { get; }

        public string NugetConfigFile { get; }

        public bool AutoDeployEnabled { get; }

        public string PackageId { get; }
    }
}