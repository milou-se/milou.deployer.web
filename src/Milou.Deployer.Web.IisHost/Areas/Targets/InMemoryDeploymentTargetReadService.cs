using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Primitives;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Structure;

namespace Milou.Deployer.Web.IisHost.Areas.Targets
{
    [UsedImplicitly]
    public class InMemoryDeploymentTargetReadService : IDeploymentTargetReadService
    {
        public async Task<DeploymentTarget> GetDeploymentTargetAsync(
            string deploymentTargetId,
            CancellationToken cancellationToken = default)
        {
            ImmutableArray<OrganizationInfo> organizations = await GetOrganizationsAsync(cancellationToken);

            DeploymentTarget foundDeploymentTarget = organizations
                .SelectMany(organizationInfo => organizationInfo.Projects)
                .SelectMany(projectInfo => projectInfo.DeploymentTargets)
                .SingleOrDefault(deploymentTarget => deploymentTarget.Id == deploymentTargetId);

            return foundDeploymentTarget;
        }

        public async Task<ImmutableArray<OrganizationInfo>> GetOrganizationsAsync(
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<OrganizationInfo> orgs = await GetTargetsAsync();

            return orgs.ToImmutableArray();
        }

        public Task<IReadOnlyCollection<OrganizationInfo>> GetTargetsAsync()
        {
            Serilog.Log.Logger.Information("Getting targets from in-memory storge");

            var targets = new List<OrganizationInfo>
            {
                new OrganizationInfo("testorg",
                    new List<ProjectInfo>
                    {
                        new ProjectInfo("testorg",
                            "testproject",
                            new List<DeploymentTarget>
                            {
                                new DeploymentTarget("TestTarget",
                                    "Test target",
                                    "MilouDeployer",
                                    true,
                                    new StringValues("*"),
                                    emailNotificationAddresses: new StringValues("noreply@localhost.local"))
                            })
                    })
            };


            return Task.FromResult<IReadOnlyCollection<OrganizationInfo>>(targets);
        }
    }
}