using System;
using JetBrains.Annotations;
using Marten.Schema;
using Milou.Deployer.Web.Marten;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    [MartenData]
    public class DeploymentTargetData
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool AllowExplicitPreRelease { get; set; }

        [PublicAPI]
        [ForeignKey(typeof(ProjectData))]
        public string ProjectId { get; set; }

        public Uri Url { get; set; }

        public string IisSiteName { get; set; }

        public bool AutoDeployEnabled { get; set; }

        public string PackageId { get; set; }

        public string PublishSettingsXml { get; set; }

        public string TargetDirectory { get; set; }

        public string WebConfigTransform { get; set; }

        public string ExcludedFilePatterns { get; set; }

        public string PublishType { get; set; }

        public string FtpPath { get; set; }

        public bool Enabled { get; set; }

        public NuGetData NuGetData { get; set; }

        public TimeSpan? MetadataTimeout { get; set; }

        public string? EnvironmentTypeId { get; set; }

        public bool? RequireEnvironmentConfig { get; set; }

        public string? EnvironmentConfiguration { get; set; }

        public bool? PackageListPrefixEnabled { get; set; }

        public string? PackageListPrefix { get; set; }
    }
}
