using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost.Areas.Targets.Controllers
{
    public class EditTargetViewOutputModel
    {
        public DeploymentTarget DeploymentTarget { get; }

        public EditTargetViewOutputModel(DeploymentTarget deploymentTarget)
        {
            DeploymentTarget = deploymentTarget;
        }
    }
}