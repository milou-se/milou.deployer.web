using System.ComponentModel.DataAnnotations;
using MediatR;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Deployment
{
    public sealed class CreateOrganization : IRequest<CreateOrganizationResult>, Validation.IValidationObject
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