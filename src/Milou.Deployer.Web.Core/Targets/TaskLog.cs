using System;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Targets
{
    [PublicAPI]
    public class TaskLog
    {
        public string DeploymentTaskId { get; set; }

        public string DeploymentTargetId { get; set; }

        public string Id { get; set; }

        public string Log { get; set; }

        public DateTime FinishedAtUtc { get; set; }
    }
}
