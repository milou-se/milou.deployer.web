using System;
using System.Collections.Generic;
using Arbor.App.Extensions;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class OrganizationInfo
    {
        public OrganizationInfo([NotNull] string organization, [CanBeNull] IEnumerable<ProjectInfo> projects)
        {
            if (string.IsNullOrWhiteSpace(organization))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(organization));
            }

            Projects = projects.SafeToReadOnlyCollection();
            Organization = organization;
        }

        public string Organization { get; }

        public IReadOnlyCollection<ProjectInfo> Projects { get; }
    }
}
