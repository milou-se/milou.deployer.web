using System;
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
            bool autoDeployEnabled = false,
            string publishSettingsXml = null,
            string targetDirectory = null)
        {
            Id = id;
            AllowExplicitPreRelease = allowExplicitPreRelease;
            Url = url;
            PackageId = packageId;
            IisSiteName = iisSiteName;
            NugetPackageSource = nugetPackageSource;
            NugetConfigFile = nugetConfigFile;
            AutoDeployEnabled = autoDeployEnabled;
            PublishSettingsXml = publishSettingsXml;
            TargetDirectory = targetDirectory;
        }

        public string Id { get; }

        public Uri Url { get; }

        public bool AllowExplicitPreRelease { get; }

        public string IisSiteName { get; }

        public string NugetPackageSource { get; }

        public string NugetConfigFile { get; }

        public bool AutoDeployEnabled { get; }

        public string PublishSettingsXml { get; }

        public string TargetDirectory { get; }

        public string PackageId { get; }
    }
}