using System;
using Milou.Deployer.Web.Core.Processing;

namespace Milou.Deployer.Web.Core.Targets
{
    public class TaskMetadata
    {
        public string DeploymentTaskId { get; set; }

        public string DeploymentTargetId { get; set; }

        public string Id { get; set; }

        public string Metadata { get; set; }

        public DateTime StartedAtUtc { get; set; }

        public DateTime FinishedAtUtc { get; set; }

        public int ExitCode { get; set; }
    }
}