using System.Collections;
using System.Collections.Immutable;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost.Areas.Targets.Views
{
    public class EditTargetViewOutputModel
    {
        public EditTargetViewOutputModel(DeploymentTarget deploymentTarget, ImmutableArray<EnvironmentType> environmentTypes)
        {
            DeploymentTarget = deploymentTarget;
            EnvironmentTypes = environmentTypes;
        }

        public DeploymentTarget DeploymentTarget { get; }

        public ImmutableArray<EnvironmentType> EnvironmentTypes { get; }
    }
}
