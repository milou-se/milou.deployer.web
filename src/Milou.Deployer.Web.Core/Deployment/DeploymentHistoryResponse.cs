using System.Collections.Generic;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentHistoryResponse
    {
        public IReadOnlyCollection<DeploymentTaskInfo> DeploymentTasks { get; }

        public DeploymentHistoryResponse(IReadOnlyCollection<DeploymentTaskInfo> deploymentTasks)
        {
            DeploymentTasks = deploymentTasks;
        }
    }
}