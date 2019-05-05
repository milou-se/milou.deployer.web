using System;
using JetBrains.Annotations;
using Marten.Schema;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
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

        public string NuGetConfigFile { get; set; }

        public string NuGetPackageSource { get; set; }

        public bool AutoDeployEnabled { get; set; }

        public string PackageId { get; set; }

        public string PublishSettingsXml { get; set; }

        public string TargetDirectory { get; set; }

        public string WebConfigTransform { get; set; }

        public string ExcludedFilePatterns { get; set; }

        public bool Enabled { get; set; }

        public string EnvironmentType { get; set; }
    }
}
