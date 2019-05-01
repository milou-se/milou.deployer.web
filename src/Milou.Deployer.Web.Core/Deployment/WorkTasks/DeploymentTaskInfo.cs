using System;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment.WorkTasks
{
    public class DeploymentTaskInfo
    {
        public DeploymentTaskInfo(
            string deploymentTaskId,
            string metadata,
            DateTime startedAtUtc,
            DateTime finishedAtUtc,
            int exitCode,
            string packageId,
            string version)
        {
            DeploymentTaskId = deploymentTaskId;
            Metadata = metadata;
            StartedAtUtc = startedAtUtc;
            FinishedAtUtc = finishedAtUtc;
            ExitCode = exitCode;
            PackageId = packageId;
            Version = version;
        }

        public string DeploymentTaskId { get; }

        public string Metadata { get; }

        [PublicAPI]
        public DateTime StartedAtUtc { get; }

        public DateTime FinishedAtUtc { get; }

        public int ExitCode { get; }

        public string PackageId { get; }

        public string Version { get; }

        public TimeSpan Duration => TimeSpan.FromSeconds((int)(FinishedAtUtc - StartedAtUtc).TotalSeconds);
    }
}
