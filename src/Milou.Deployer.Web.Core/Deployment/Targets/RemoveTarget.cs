using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public class RemoveTarget : IRequest
    {
        [Required]
        public string DeploymentTargetId { get; }

        public RemoveTarget(string deploymentTargetId) => DeploymentTargetId = deploymentTargetId;
    }
}
