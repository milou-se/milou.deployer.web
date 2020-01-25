using System;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Validation;
using MediatR;
using Milou.Deployer.Web.Core.Deployment.Messages;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class CreateTarget : IRequest<CreateTargetResult>, IValidationObject
    {
        public CreateTarget(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }

        public string Name { get; }

        public override string ToString()
        {
            return "";
        }

        public bool IsValid => Id.HasValue() && Name.HasValue() &&
                               !Id.Equals(Constants.NotAvailable, StringComparison.OrdinalIgnoreCase);
    }
}
