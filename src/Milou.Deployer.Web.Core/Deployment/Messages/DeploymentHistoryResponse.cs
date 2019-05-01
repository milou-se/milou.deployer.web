using System.Collections.Generic;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;

namespace Milou.Deployer.Web.Core.Deployment.Messages
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
