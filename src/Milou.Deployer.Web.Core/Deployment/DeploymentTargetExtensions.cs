using System;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Agent
{
    public static class DeploymentTargetExtensions
    {
        public static string? GetEnvironmentConfiguration(this DeploymentTarget deploymentTarget)
        {
            string? targetEnvironmentConfigName = !string.IsNullOrWhiteSpace(deploymentTarget.EnvironmentType?.Id)
                                                  && !string.IsNullOrWhiteSpace(deploymentTarget.EnvironmentType?.Name)

                ? deploymentTarget.EnvironmentType.Name
                : deploymentTarget.EnvironmentConfiguration;

            if (string.Equals(EnvironmentType.Unknown.Name, deploymentTarget.EnvironmentConfiguration, StringComparison.OrdinalIgnoreCase))
            {
                targetEnvironmentConfigName = null;
            }

            if (string.IsNullOrWhiteSpace(deploymentTarget.EnvironmentConfiguration))
            {
                targetEnvironmentConfigName = null;
            }

            return targetEnvironmentConfigName;
        }
    }
}