using System.Collections.Generic;
using System.Linq;
using Arbor.App.Extensions;
using Milou.Deployer.Web.Core.Deployment.Targets;

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
