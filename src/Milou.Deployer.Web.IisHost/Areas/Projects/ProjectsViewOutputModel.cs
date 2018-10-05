using System.Collections.Immutable;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Targets;

namespace Milou.Deployer.Web.IisHost.Areas.Projects
{
    public class ProjectsViewOutputModel
    {
        public ImmutableArray<ProjectInfo> Projects { get; }
        public CreateProjectResult CreateProjectResult { get; }
        public string OrganizationId { get; }

        public ProjectsViewOutputModel(
            ImmutableArray<ProjectInfo> organizations,
            CreateProjectResult createProjectResult,
            string organizationId)
        {
            Projects = organizations;
            CreateProjectResult = createProjectResult;
            OrganizationId = organizationId;
        }
    }
}