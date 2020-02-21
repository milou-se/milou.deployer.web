using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Arbor.App.Extensions.IO;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment.Packages;
using Newtonsoft.Json;
using NuGet.Versioning;

namespace Milou.Deployer.Web.Core.Deployment.WorkTasks
{
    public class DeploymentTask
    {
        public DeploymentTask(
            [NotNull] string packageVersion,
            [NotNull] string deploymentTargetId,
            Guid deploymentTaskId,
            string startedBy)
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
            string packageId = parts[0];

            var version = SemanticVersion.Parse(parts.Last());

            SemanticVersion = version;
            PackageId = packageId;
            DeploymentTargetId = deploymentTargetId;
            StartedBy = startedBy;
            DeploymentTaskId =
                $"{DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture).Replace(":", "_", StringComparison.InvariantCulture)}_{deploymentTaskId.ToString().Substring(0, 8)}";
        }

        public string StartedBy { get; }

        public DeploymentTask(
            [NotNull] PackageVersion packageVersion,
            [NotNull] string deploymentTargetId,
            Guid deploymentTaskId,
            string startedBy)
        {
            if (packageVersion == null)
            {
                throw new ArgumentNullException(nameof(packageVersion));
            }

            SemanticVersion = packageVersion.Version;
            PackageId = packageVersion.PackageId;
            DeploymentTargetId = deploymentTargetId;
            StartedBy = startedBy;
            DeploymentTaskId =
                $"{DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture).Replace(":", "_", StringComparison.Ordinal)}_{deploymentTaskId.ToString().Substring(0, 8)}";
        }

        [JsonIgnore]
        public List<DirectoryInfo> TempDirectories { get; } = new List<DirectoryInfo>();

        [JsonIgnore]
        public List<TempFile> TempFiles { get; } = new List<TempFile>();

        public SemanticVersion SemanticVersion { get; }

        public string DeploymentTargetId { get; }

        public string PackageId { get; }

        public string DeploymentTaskId { get; }

        public DateTime EnqueuedAtUtc { get; set; }

        [PublicAPI]
        [JsonIgnore]
        public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Created;

        [JsonIgnore]
        public BlockingCollection<(string, WorkTaskStatus)> MessageQueue { get; } =
            new BlockingCollection<(string, WorkTaskStatus)>();

        public void Log(string message)
        {
            if (MessageQueue.IsAddingCompleted)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            MessageQueue.Add((message, Status));

            if (Status == WorkTaskStatus.Done || Status == WorkTaskStatus.Failed)
            {
                MessageQueue.CompleteAdding();
            }
        }

        public override string ToString() => $"{nameof(SemanticVersion)}: {SemanticVersion.ToNormalizedString()}, {nameof(DeploymentTargetId)}: {DeploymentTargetId}, {nameof(PackageId)}: {PackageId}, {nameof(DeploymentTaskId)}: {DeploymentTaskId}";
    }
}
