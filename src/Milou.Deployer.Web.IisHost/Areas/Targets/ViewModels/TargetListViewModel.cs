using System.Collections.Immutable;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost.Areas.Targets.ViewModels
{
    public class TargetListViewModel
    {
        public TargetListViewModel(ImmutableArray<DeploymentTarget> targets)
        {
            Targets = targets;
        }

        public ImmutableArray<DeploymentTarget> Targets { get; }
    }
}
