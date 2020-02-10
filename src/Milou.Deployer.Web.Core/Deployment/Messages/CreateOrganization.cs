using System.ComponentModel.DataAnnotations;
using Arbor.App.Extensions;
using MediatR;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public sealed class CreateOrganization : IRequest<CreateOrganizationResult>
    {
        public CreateOrganization(string id)
        {
            Id = id;
        }

        [Required]
        public string Id { get; }

        public bool IsValid => Id.HasValue();
    }
}
