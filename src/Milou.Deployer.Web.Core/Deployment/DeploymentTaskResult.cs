using System;
using Milou.Deployer.Web.Core.Processing;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentTaskResult
    {
        public DeploymentTaskResult(
            string deploymentTaskId,
            string deploymentTargetId,
            ExitCode exitCode,
            DateTime startedAtUtc,
            DateTime finishedAtUtc,
            string metadata)
        {
            DeploymentTaskId = deploymentTaskId;
            DeploymentTargetId = deploymentTargetId;
            ExitCode = exitCode;
            StartedAtUtc = startedAtUtc;
            FinishedAtUtc = finishedAtUtc;
            Metadata = metadata;
        }

        public string DeploymentTaskId { get; }

        public string DeploymentTargetId { get; }

        public ExitCode ExitCode { get; }

        public DateTime StartedAtUtc { get; }

        public DateTime FinishedAtUtc { get; }

        public string Metadata { get; }

        public override string ToString()
        {
            return $"{nameof(DeploymentTaskId)}: {DeploymentTaskId}, {nameof(DeploymentTargetId)}: {DeploymentTargetId}, {nameof(ExitCode)}: {ExitCode}, {nameof(StartedAtUtc)}: {StartedAtUtc}, {nameof(FinishedAtUtc)}: {FinishedAtUtc}, {nameof(Metadata)}: {Metadata}";
        }
    }
}