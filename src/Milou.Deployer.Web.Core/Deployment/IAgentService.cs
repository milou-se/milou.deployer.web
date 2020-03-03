using System.Threading;
using System.Threading.Tasks;
using Milou.Deployer.Web.Agent;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;

namespace Milou.Deployer.Web.Core.Deployment
{
    public interface IAgentService
    {
        Task<IDeploymentPackageAgent> GetAgentForDeploymentTask(DeploymentTask deploymentTask, CancellationToken cancellationToken);
    }
}