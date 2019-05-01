using System.Collections.Generic;
using System.Linq;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Targets
{
    public class OrganizationViewModel
    {
        public OrganizationViewModel(OrganizationInfo organization)
        {
            Organization = organization.Organization;
            Projects = organization.Projects.Select(project => new ProjectViewModel(project))
                .SafeToReadOnlyCollection();
        }

        public string Organization { get; }

        public IReadOnlyCollection<ProjectViewModel> Projects { get; }
    }
}
