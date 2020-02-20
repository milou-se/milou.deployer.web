using System.Threading;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Core.Deployment.Sources
{
    public interface IDeploymentTargetService
    {
        Task<DeploymentTarget> GetDeploymentTargetAsync(
            string deploymentTargetId,
            CancellationToken cancellationToken = default);
    }
}