using System;
using System.Collections.Generic;
using System.Linq;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Structure;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels;

namespace Milou.Deployer.Web.IisHost.Areas.Targets
{
    public class ProjectViewModel
    {
        public ProjectViewModel(ProjectInfo projectInfo)
        {
            ProjectName = projectInfo?.ProjectInvariantName ?? throw new ArgumentNullException(nameof(projectInfo));
            Targets =
                projectInfo.DeploymentTargets.Select(target => new DeploymentTargetViewOutputModel(target))
                    .SafeToReadOnlyCollection();
        }

        public string ProjectName { get; }

        public IReadOnlyCollection<DeploymentTargetViewOutputModel> Targets { get; }
    }
}