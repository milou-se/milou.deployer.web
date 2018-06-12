using System;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentTaskInfo
    {
        public DeploymentTaskInfo(
            string deploymentTaskId,
            string metadata,
            DateTime startedAtUtc,
            DateTime finishedAtUtc,
            int exitCode)
        {
            DeploymentTaskId = deploymentTaskId;
            Metadata = metadata;
            StartedAtUtc = startedAtUtc;
            FinishedAtUtc = finishedAtUtc;
            ExitCode = exitCode;
        }

        public string DeploymentTaskId { get; }

        public string Metadata { get; }

        public DateTime StartedAtUtc { get; }

        public DateTime FinishedAtUtc { get; }

        public int ExitCode { get; }
    }
}