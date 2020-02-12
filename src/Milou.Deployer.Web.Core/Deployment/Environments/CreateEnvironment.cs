using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Environments
{
    public class CreateEnvironment : IRequest<CreateEnvironmentResult>
    {
        public string EnvironmentTypeId { get; set; }

        public string EnvironmentTypeName { get; set; }

        public string PreReleaseBehavior { get; set; }
    }
}