using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NuGet.Versioning;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentTask
    {
        public DeploymentTask(
            [NotNull] string packageVersion,
            [NotNull] string deploymentTargetId,
            Guid deploymentTaskId)
        {
            if (string.IsNullOrWhiteSpace(packageVersion))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageVersion));
            }

            if (string.IsNullOrWhiteSpace(deploymentTargetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(deploymentTargetId));
            }

            var parts = packageVersion.Split(' ');
            var packageId = parts[0];

            var version = SemanticVersion.Parse(parts.Last());

            SemanticVersion = version;
            PackageId = packageId;
            DeploymentTargetId = deploymentTargetId;
            DeploymentTaskId =
                $"{DateTime.UtcNow.ToString("O").Replace(":", "_")}_{deploymentTaskId.ToString().Substring(0, 8)}";
        }

        public DeploymentTask(
            [NotNull] PackageVersion packageVersion,
            [NotNull] string deploymentTargetId,
            Guid deploymentTaskId)
        {
            if (packageVersion == null)
            {
                throw new ArgumentNullException(nameof(packageVersion));
            }

            SemanticVersion = packageVersion.Version;
            PackageId = packageVersion.PackageId;
            DeploymentTargetId = deploymentTargetId;
            DeploymentTaskId =
                $"{DateTime.UtcNow.ToString("O").Replace(":", "_")}_{deploymentTaskId.ToString().Substring(0, 8)}";
        }

        public List<DirectoryInfo> TempDirectories { get; } = new List<DirectoryInfo>();

        public List<FileInfo> TempFiles { get; } = new List<FileInfo>();

        public SemanticVersion SemanticVersion { get; }

        public string DeploymentTargetId { get; }

        public string PackageId { get; }

        public string DeploymentTaskId { get; }

        [PublicAPI]
        public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Created;

        public BlockingCollection<(string, WorkTaskStatus)> MessageQueue { get; } =
            new BlockingCollection<(string, WorkTaskStatus)>();

        public void Log(string message)
        {
            if (MessageQueue.IsAddingCompleted)
            {
                return;
            }

            MessageQueue.Add((message, Status));

            if (Status == WorkTaskStatus.Done || Status == WorkTaskStatus.Failed)
            {
                MessageQueue.CompleteAdding();
            }
        }

        public override string ToString()
        {
            return
                $"{nameof(SemanticVersion)}: {SemanticVersion.ToNormalizedString()}, {nameof(DeploymentTargetId)}: {DeploymentTargetId}, {nameof(PackageId)}: {PackageId}, {nameof(DeploymentTaskId)}: {DeploymentTaskId}";
        }
    }
}
