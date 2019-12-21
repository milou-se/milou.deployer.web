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

        Task<ImmutableArray<DeploymentTarget>> GetDeploymentTargetsAsync(TargetOptions options = default, CancellationToken stoppingToken = default);

        Task<ImmutableArray<ProjectInfo>> GetProjectsAsync(
            string organizationId,
            CancellationToken cancellationToken = default);
    }
}
