using System;
using System.Collections.Generic;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.ViewOutputModels
{
    public class DeploymentTargetViewOutputModel
    {
        public DeploymentTargetViewOutputModel(DeploymentTarget target)
        {
            Target = target;

            ConfigurationPairs = GetProperties(target);
        }

        public IReadOnlyCollection<StringPair> ConfigurationPairs { get; }

        public DeploymentTarget Target { get; }

        private static IReadOnlyCollection<StringPair> GetProperties(DeploymentTarget deploymentTarget)
        {
            return StaticKeyValueConfigurationManager.AppSettings.AllValues.Where(
                kv =>
                    kv.Key.StartsWith(
                        $"{ConfigurationConstants.DeployerTargetPrefix}:{deploymentTarget.Id}",
                        StringComparison.InvariantCultureIgnoreCase)).SafeToReadOnlyCollection();
        }
    }
}