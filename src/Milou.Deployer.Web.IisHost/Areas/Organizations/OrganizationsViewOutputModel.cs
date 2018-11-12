using System.Collections.Immutable;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Targets;

namespace Milou.Deployer.Web.IisHost.Areas.Organizations
{
    public class OrganizationsViewOutputModel
    {
        public ImmutableArray<OrganizationInfo> Organizations { get; }
        public CreateOrganizationResult CreateOrganizationResult { get; }

        public OrganizationsViewOutputModel(
            ImmutableArray<OrganizationInfo> organizations,
            CreateOrganizationResult createOrganizationResult)
        {
            Organizations = organizations;
            CreateOrganizationResult = createOrganizationResult;
        }
    }
}