using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class DeploymentFinishedNotification : INotification
    {
        public DeploymentFinishedNotification(
            [NotNull] DeploymentTask deploymentTask,
            LogItem[] logLines,
            DateTime finishedAtUtc)
        {
            LogLines = logLines.ToImmutableArray();
            FinishedAtUtc = finishedAtUtc;
            DeploymentTask = deploymentTask ?? throw new ArgumentNullException(nameof(deploymentTask));
        }

        public ImmutableArray<LogItem> LogLines { get; }

        public DeploymentTask DeploymentTask { get; }

        public DateTime FinishedAtUtc { get; }
    }
}
