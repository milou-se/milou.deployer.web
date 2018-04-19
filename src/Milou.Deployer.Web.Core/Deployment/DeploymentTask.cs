using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NuGet.Versioning;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentTask
    {
        public DeploymentTask([NotNull] string packageVersion, [NotNull] string deploymentTargetId, Guid deploymentTaskId)
        {
            if (string.IsNullOrWhiteSpace(packageVersion))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageVersion));
            }

            if (string.IsNullOrWhiteSpace(deploymentTargetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(deploymentTargetId));
            }

            string[] parts = packageVersion.Split(' ');
            string packageId = parts.First();

            SemanticVersion version = SemanticVersion.Parse(parts.Last());

            SemanticVersion = version;
            PackageId = packageId;
            DeploymentTargetId = deploymentTargetId;
            DeploymentTaskId = $"{DateTime.UtcNow.ToString("O").Replace(":", "_")}_{deploymentTaskId.ToString().Substring(0, 8)}";
        }

        public List<DirectoryInfo> TempDirectories { get; } = new List<DirectoryInfo>();

        public List<FileInfo> TempFiles { get; } = new List<FileInfo>();

        public SemanticVersion SemanticVersion { get; }

        public string DeploymentTargetId { get; }

        public string PackageId { get; }

        public string DeploymentTaskId { get; }

        public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Created;

        public void Log(string message) => LogActions.ForEach(action => action.Invoke(message, Status));

        public List<Action<string, WorkTaskStatus>> LogActions { get; } = new List<Action<string, WorkTaskStatus>>();

        public override string ToString()
        {
            return $"{nameof(SemanticVersion)}: {SemanticVersion.ToNormalizedString()}, {nameof(DeploymentTargetId)}: {DeploymentTargetId}, {nameof(PackageId)}: {PackageId}, {nameof(DeploymentTaskId)}: {DeploymentTaskId}";
        }
    }
}