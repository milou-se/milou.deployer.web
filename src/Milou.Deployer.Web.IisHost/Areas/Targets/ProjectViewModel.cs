using System;
using System.Collections.Generic;
using System.Linq;
using Arbor.App.Extensions;
using Arbor.App.Extensions.ExtensionMethods;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels;

namespace Milou.Deployer.Web.IisHost.Areas.Targets
{
    public class ProjectViewModel
    {
        public ProjectViewModel(ProjectInfo projectInfo)
        {
            ProjectName = projectInfo?.ProjectInvariantName ?? throw new ArgumentNullException(nameof(projectInfo));
            Targets =
                projectInfo.DeploymentTargets
                    .Select(target =>
                        new DeploymentTargetViewOutputModel(target,
                            Array.Empty<StringPair>())) // TODO improve view models
                    .SafeToReadOnlyCollection();
        }

        public string ProjectName { get; }

        public IReadOnlyCollection<DeploymentTargetViewOutputModel> Targets { get; }
    }
}
