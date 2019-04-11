using System;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Targets
{
    [PublicAPI]
    public class TaskMetadata
    {
        public string PackageId { get; set; }

        public string DeploymentTaskId { get; set; }

        public string DeploymentTargetId { get; set; }

        public string Id { get; set; }

        public string Metadata { get; set; }

        public DateTime StartedAtUtc { get; set; }

        public DateTime FinishedAtUtc { get; set; }

        public int ExitCode { get; set; }

        public string Version { get; set; }
    }
}
