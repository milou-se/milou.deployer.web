using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public class DeploymentTargetIds
    {
        public IReadOnlyCollection<string> DeploymentWorkerIds { get; }

        public DeploymentTargetIds([NotNull] IReadOnlyCollection<string> deploymentWorkerIds)
        {
            DeploymentWorkerIds = deploymentWorkerIds ?? throw new ArgumentNullException(nameof(deploymentWorkerIds));
        }
    }
}