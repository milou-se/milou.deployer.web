using System.ComponentModel.DataAnnotations;
using MediatR;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.Core.Deployment.Messages
{
    public sealed class CreateOrganization : IRequest<CreateOrganizationResult>, IValidationObject
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
