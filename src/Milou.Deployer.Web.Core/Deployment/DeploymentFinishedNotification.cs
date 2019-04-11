using System;
using JetBrains.Annotations;
using MediatR;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentFinishedNotification : INotification
    {
        public DeploymentFinishedNotification(
            [NotNull] DeploymentTask deploymentTask,
            string log,
            DateTime finishedAtUtc)
        {
            Log = log;
            FinishedAtUtc = finishedAtUtc;
            DeploymentTask = deploymentTask ?? throw new ArgumentNullException(nameof(deploymentTask));
        }

        public string Log { get; }

        public DeploymentTask DeploymentTask { get; }

        public DateTime FinishedAtUtc { get; }
    }
}
