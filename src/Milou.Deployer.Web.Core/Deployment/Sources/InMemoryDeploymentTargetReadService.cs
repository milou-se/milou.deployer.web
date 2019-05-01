using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Primitives;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Serilog;

namespace Milou.Deployer.Web.Core.Deployment.Sources
{
    [UsedImplicitly]
    public class InMemoryDeploymentTargetReadService : IDeploymentTargetReadService
    {
        private readonly Func<Task<IReadOnlyCollection<OrganizationInfo>>> _dataCreator;
        private readonly ILogger _logger;

        public InMemoryDeploymentTargetReadService(
            ILogger logger,
            Func<Task<IReadOnlyCollection<OrganizationInfo>>> dataCreator = null)
        {
            _logger = logger;
            _dataCreator = dataCreator;
        }

        [PublicAPI]
        public Task<IReadOnlyCollection<OrganizationInfo>> GetTargetsAsync()
        {
            if (_dataCreator != null)
            {
                return _dataCreator.Invoke();
            }

            _logger.Information("Getting targets from in-memory storage");

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
                                    null,
                                    true,
                                    new StringValues("*"),
                                    targetDirectory: Path.Combine(
                                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                        "Milou.Deployer.Web",
                                        "TestTarget"),
                                    emailNotificationAddresses: new StringValues("noreply@localhost.local"))
                            })
                    })
            };

            return Task.FromResult<IReadOnlyCollection<OrganizationInfo>>(targets);
        }

        public async Task<DeploymentTarget> GetDeploymentTargetAsync(
            string deploymentTargetId,
            CancellationToken cancellationToken = default)
        {
            var organizations = await GetOrganizationsAsync(cancellationToken);

            var foundDeploymentTarget = organizations
                .SelectMany(organizationInfo => organizationInfo.Projects)
                .SelectMany(projectInfo => projectInfo.DeploymentTargets)
                .SingleOrDefault(deploymentTarget => deploymentTarget.Id == deploymentTargetId);

            return foundDeploymentTarget;
        }

        public async Task<ImmutableArray<OrganizationInfo>> GetOrganizationsAsync(
            CancellationToken cancellationToken = default)
        {
            var organizations = await GetTargetsAsync();

            return organizations.ToImmutableArray();
        }

        public async Task<ImmutableArray<DeploymentTarget>> GetDeploymentTargetsAsync(CancellationToken stoppingToken)
        {
            var organizations = await GetOrganizationsAsync(stoppingToken);

            return organizations
                .SelectMany(organizationInfo => organizationInfo.Projects)
                .SelectMany(projectInfo => projectInfo.DeploymentTargets).ToImmutableArray();
        }

        public Task<ImmutableArray<ProjectInfo>> GetProjectsAsync(
            string organizationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ImmutableArray<ProjectInfo>.Empty);
        }
    }
}
