using System;
using System.ComponentModel.DataAnnotations;
using Arbor.App.Extensions;
using Arbor.App.Extensions.ExtensionMethods;
using MediatR;
using Milou.Deployer.Web.Core.Deployment.Messages;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class CreateTarget : IRequest<CreateTargetResult>
    {
        public CreateTarget(string id, string name)
        {
            Id = id?.Trim();
            Name = name?.Trim();
        }

        [Required]
        public string Id { get; }

        [Required]
        public string Name { get; }

        public override string ToString()
        {
            return Id;
        }

        public bool IsValid => Id.HasValue() && Name.HasValue() &&
                               !Id.Equals(Constants.NotAvailable, StringComparison.OrdinalIgnoreCase);
    }
}
