using Arbor.App.Extensions;
using MediatR;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public class CreateProject : IRequest<CreateProjectResult>
    {
        public CreateProject(string id, string organizationId)
        {
            Id = id;
            OrganizationId = organizationId;
        }

        public string Id { get; }

        public string OrganizationId { get; }

        public bool IsValid => Id.HasValue() && OrganizationId.HasValue();
    }
}
