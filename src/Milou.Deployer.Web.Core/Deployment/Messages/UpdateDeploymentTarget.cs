using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using MediatR;
using Milou.Deployer.Core.Deployment;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class UpdateDeploymentTarget : IRequest<UpdateDeploymentTargetResult>, IValidationObject, IValidatableObject
    {
        public UpdateDeploymentTarget(
            string id,
            bool allowExplicitPreRelease,
            string url,
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
            _ = Uri.TryCreate(url, UriKind.Absolute, out Uri uri);
            Url = uri;
            PackageId = packageId;
            ExcludedFilePatterns = excludedFilePatterns;
            _ = PublishType.TryParseOrDefault(publishType, out var foundPublishType);
            _ = FtpPath.TryParse(ftpPath, FileSystemType.Directory, out var path);
            PublishType = foundPublishType;
            FtpPath = path;
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

        public EnvironmentType? EnvironmentType { get; }

        public string Id { get; }

        public Uri? Url { get; }

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

        public PublishType PublishType { get; }

        public FtpPath? FtpPath { get; }

        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(Url)}: {Url}, {nameof(AllowExplicitPreRelease)}: {AllowExplicitPreRelease}, {nameof(IisSiteName)}: {IisSiteName}, {nameof(NugetPackageSource)}: {NugetPackageSource}, {nameof(NugetConfigFile)}: {NugetConfigFile}, {nameof(AutoDeployEnabled)}: {AutoDeployEnabled}, {nameof(PublishSettingsXml)}: {PublishSettingsXml}, {nameof(TargetDirectory)}: {TargetDirectory}, {nameof(PackageId)}: {PackageId}, {nameof(IsValid)}: {IsValid}";
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Url is null)
            {
                yield return new ValidationResult("URL must be defined", new []{nameof(Url)});
            }
        }

        public bool IsValid { get; }

        public TimeSpan? PackageListTimeout { get; }
    }
}
