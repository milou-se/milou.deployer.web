using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Milou.Deployer.Web.Core.Deployment.Targets;

namespace Milou.Deployer.Web.Core.Deployment.Sources
{
    public interface IDeploymentTargetReadService
    {
        Task<DeploymentTarget> GetDeploymentTargetAsync(
            string deploymentTargetId,
            CancellationToken cancellationToken = default);

        Task<ImmutableArray<OrganizationInfo>> GetOrganizationsAsync(CancellationToken cancellationToken = default);

        Task<ImmutableArray<DeploymentTarget>> GetDeploymentTargetsAsync(CancellationToken stoppingToken);

        Task<ImmutableArray<ProjectInfo>> GetProjectsAsync(
            string organizationId,
            CancellationToken cancellationToken = default);
    }
}
