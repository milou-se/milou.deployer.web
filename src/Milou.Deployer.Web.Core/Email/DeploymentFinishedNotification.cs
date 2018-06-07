using System;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Core.Email
{
    public class DeploymentFinishedNotification : INotification
    {
        public DeploymentFinishedNotification(
            [NotNull] DeploymentTask deploymentTask,
            string log)
        {
            Log = log;
            DeploymentTask = deploymentTask ?? throw new ArgumentNullException(nameof(deploymentTask));
        }

        public string Log { get; }

        public DeploymentTask DeploymentTask { get; }
    }
}