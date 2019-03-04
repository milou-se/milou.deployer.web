using System;
using System.Collections.Generic;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels
{
    public class DeploymentTargetViewOutputModel
    {
        public DeploymentTargetViewOutputModel(
            [NotNull] DeploymentTarget target,
            [NotNull] IReadOnlyCollection<StringPair> configurationPairs)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            ConfigurationPairs = configurationPairs ?? throw new ArgumentNullException(nameof(configurationPairs));
        }

        public IReadOnlyCollection<StringPair> ConfigurationPairs { get; }

        public DeploymentTarget Target { get; }
    }
}
