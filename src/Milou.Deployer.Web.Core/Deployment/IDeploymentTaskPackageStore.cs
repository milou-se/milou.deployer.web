using System.Threading;
using System.Threading.Tasks;
using Milou.Deployer.Web.Agent;

namespace Milou.Deployer.Web.Core.Deployment
{
    public interface IDeploymentTaskPackageStore
    {
        Task<DeploymentTaskPackage> GetDeploymentTaskPackageAsync(string deploymentTaskId, CancellationToken cancellationToken);
    }

}