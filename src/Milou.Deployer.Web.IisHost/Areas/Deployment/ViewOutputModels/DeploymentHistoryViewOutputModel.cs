using System.Collections.Generic;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels
{
    public class DeploymentHistoryViewOutputModel
    {
        public DeploymentHistoryViewOutputModel(IReadOnlyCollection<DeploymentTaskInfo> deploymentTasks)
        {
            DeploymentTasks = deploymentTasks;
        }

        public IReadOnlyCollection<DeploymentTaskInfo> DeploymentTasks { get; }
    }
}
