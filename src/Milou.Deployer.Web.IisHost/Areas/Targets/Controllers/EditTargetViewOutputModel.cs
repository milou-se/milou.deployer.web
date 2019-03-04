using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost.Areas.Targets.Controllers
{
    public class EditTargetViewOutputModel
    {
        public EditTargetViewOutputModel(DeploymentTarget deploymentTarget)
        {
            DeploymentTarget = deploymentTarget;
        }

        public DeploymentTarget DeploymentTarget { get; }
    }
}
