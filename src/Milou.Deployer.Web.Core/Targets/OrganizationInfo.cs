using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Targets
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