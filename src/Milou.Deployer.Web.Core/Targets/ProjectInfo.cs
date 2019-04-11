using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Core.Targets
{
    [PublicAPI]
    public class ProjectInfo
    {
        public ProjectInfo(
            [NotNull] string organizationInvariantName,
            [NotNull] string projectInvariantName,
            [CanBeNull] IEnumerable<DeploymentTarget> deploymentTargets)
        {
            if (string.IsNullOrWhiteSpace(projectInvariantName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(projectInvariantName));
            }

            if (!NameHelper.IsNameValid(organizationInvariantName))
            {
                throw new ArgumentException("Organization name is invalid", nameof(organizationInvariantName));
            }

            OrganizationInvariantName = organizationInvariantName;
            DeploymentTargets = deploymentTargets;
            ProjectInvariantName = projectInvariantName;
            ProjectDisplayName = projectInvariantName;
        }

        public string ProjectDisplayName { get; }

        public string OrganizationInvariantName { get; }

        public IEnumerable<DeploymentTarget> DeploymentTargets { get; }

        public string ProjectInvariantName { get; }

        public string ProjectFullName => NameHelper.GetFullProjectName(OrganizationInvariantName, ProjectInvariantName);
    }
}
