using MediatR;
using Milou.Deployer.Web.Agent;

namespace Milou.Deployer.Web.Core.Deployment
{
    public class CreateDeploymentTaskPackage : IRequest<Unit>
    {
        public DeploymentTaskPackage DeploymentTaskPackage { get; }

        public CreateDeploymentTaskPackage(DeploymentTaskPackage deploymentTaskPackage)
        {
            DeploymentTaskPackage = deploymentTaskPackage;
        }
    }
}