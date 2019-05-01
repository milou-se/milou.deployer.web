using System.Collections.Immutable;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Targets;

namespace Milou.Deployer.Web.IisHost.Areas.Organizations
{
    public class OrganizationsViewOutputModel
    {
        public OrganizationsViewOutputModel(
            ImmutableArray<OrganizationInfo> organizations,
            CreateOrganizationResult createOrganizationResult)
        {
            Organizations = organizations;
            CreateOrganizationResult = createOrganizationResult;
        }

        public ImmutableArray<OrganizationInfo> Organizations { get; }
        public CreateOrganizationResult CreateOrganizationResult { get; }
    }
}
