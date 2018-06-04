using MediatR;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Targets
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

        public bool IsValid => Id.HasValue() && Name.HasValue();
    }
}