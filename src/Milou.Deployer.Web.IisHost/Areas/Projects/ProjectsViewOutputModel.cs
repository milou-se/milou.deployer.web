using System.Collections.Immutable;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Targets;

namespace Milou.Deployer.Web.IisHost.Areas.Projects
{
    public class ProjectsViewOutputModel
    {
        public ProjectsViewOutputModel(
            ImmutableArray<ProjectInfo> organizations,
            CreateProjectResult createProjectResult,
            string organizationId)
        {
            Projects = organizations;
            CreateProjectResult = createProjectResult;
            OrganizationId = organizationId;
        }

        public ImmutableArray<ProjectInfo> Projects { get; }
        public CreateProjectResult CreateProjectResult { get; }
        public string OrganizationId { get; }
    }
}
