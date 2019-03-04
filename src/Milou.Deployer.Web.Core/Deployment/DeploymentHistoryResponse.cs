using System.Collections.Generic;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class DeploymentHistoryResponse
    {
        public DeploymentHistoryResponse(IReadOnlyCollection<DeploymentTaskInfo> deploymentTasks)
        {
            DeploymentTasks = deploymentTasks;
        }

        public IReadOnlyCollection<DeploymentTaskInfo> DeploymentTasks { get; }
    }
}
