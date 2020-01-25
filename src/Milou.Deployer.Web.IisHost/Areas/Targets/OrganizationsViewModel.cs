using System;
using System.Collections.Generic;
using System.Linq;
using Arbor.App.Extensions;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Deployment.Targets;

namespace Milou.Deployer.Web.IisHost.Areas.Targets
{
    public class OrganizationsViewModel
    {
        public OrganizationsViewModel([NotNull] IReadOnlyCollection<OrganizationInfo> organizations)
        {
            if (organizations == null)
            {
                throw new ArgumentNullException(nameof(organizations));
            }

            Organizations =
                organizations.Select(organization => new OrganizationViewModel(organization))
                    .OrderBy(organizationInfo => organizationInfo.Organization)
                    .SafeToReadOnlyCollection();
        }

        public IReadOnlyCollection<OrganizationViewModel> Organizations { get; }
    }
}
