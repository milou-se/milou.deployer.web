using System;
using MediatR;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class UpdateDeploymentTarget : IRequest<UpdateDeploymentTargetResult>, IValidationObject
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
            string targetDirectory = null,
            string webConfigTransform = null,
            string excludedFilePatterns = null,
            string environmentType = null,
            string packageListTimeout = null,
            string publishType = null,
            string ftpPath = null)
        {
            Id = id;
            AllowExplicitPreRelease = allowExplicitPreRelease;
            Url = url;
            PackageId = packageId;
            ExcludedFilePatterns = excludedFilePatterns;
            PublishType = publishType;
            FtpPath = ftpPath;
            IisSiteName = iisSiteName;
            NugetPackageSource = nugetPackageSource;
            NugetConfigFile = nugetConfigFile;
            AutoDeployEnabled = autoDeployEnabled;
            PublishSettingsXml = publishSettingsXml;
            TargetDirectory = targetDirectory;
            WebConfigTransform = webConfigTransform;
            IsValid = !string.IsNullOrWhiteSpace(Id);
            EnvironmentType = EnvironmentType.Parse(environmentType);

            if (TimeSpan.TryParse(packageListTimeout, out TimeSpan timeout) && timeout.TotalSeconds > 0.5D)
            {
                PackageListTimeout = timeout;
            }
        }

        public EnvironmentType EnvironmentType { get; }

        public string Id { get; }

        public Uri Url { get; }

        public bool AllowExplicitPreRelease { get; }

        public string IisSiteName { get; }

        public string NugetPackageSource { get; }

        public string NugetConfigFile { get; }

        public bool AutoDeployEnabled { get; }

        public string PublishSettingsXml { get; }

        public string TargetDirectory { get; }

        public string WebConfigTransform { get; }

        public string PackageId { get; }

        public string ExcludedFilePatterns { get; }
        public string PublishType { get; }
        public string FtpPath { get; }

        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(Url)}: {Url}, {nameof(AllowExplicitPreRelease)}: {AllowExplicitPreRelease}, {nameof(IisSiteName)}: {IisSiteName}, {nameof(NugetPackageSource)}: {NugetPackageSource}, {nameof(NugetConfigFile)}: {NugetConfigFile}, {nameof(AutoDeployEnabled)}: {AutoDeployEnabled}, {nameof(PublishSettingsXml)}: {PublishSettingsXml}, {nameof(TargetDirectory)}: {TargetDirectory}, {nameof(PackageId)}: {PackageId}, {nameof(IsValid)}: {IsValid}";
        }

        public bool IsValid { get; }

        public TimeSpan? PackageListTimeout { get; }
    }
}
