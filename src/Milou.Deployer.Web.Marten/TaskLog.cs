using System;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Marten
{
    [MartenData]
    [PublicAPI]
    public class TaskLog
    {
        public string DeploymentTaskId { get; set; }

        public string DeploymentTargetId { get; set; }

        public string Id { get; set; }

        public DateTime FinishedAtUtc { get; set; }
    }
}
