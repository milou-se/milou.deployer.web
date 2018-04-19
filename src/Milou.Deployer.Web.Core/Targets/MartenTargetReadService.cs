using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Marten;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Structure;
using Serilog;

namespace Milou.Deployer.Web.Core.Targets
{
    [UsedImplicitly]
    public class MartenTargetReadService : IDeploymentTargetReadService, IDeploymentTargetWriteService
    {
        private readonly IDocumentStore _documentStore;
        private ILogger _logger;

        public MartenTargetReadService([NotNull] IDocumentStore documentStore, ILogger logger)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            _logger = logger;
        }

        public async Task<DeploymentTarget> GetDeploymentTargetAsync(
            [NotNull] string deploymentTargetId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(deploymentTargetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(deploymentTargetId));
            }

            using (IQuerySession session = _documentStore.QuerySession())
            {
                DeploymentTargetData deploymentTargetData = await session.Query<DeploymentTargetData>()
                    .SingleOrDefaultAsync<DeploymentTargetData>(cancellationToken);

                DeploymentTarget deploymentTarget = MapDataToTarget(deploymentTargetData);

                return deploymentTarget;
            }
        }

        public async Task<ImmutableArray<OrganizationInfo>> GetOrganizationsAsync(
            CancellationToken cancellationToken = default)
        {
            using (IQuerySession session = _documentStore.QuerySession())
            {
                IReadOnlyList<DeploymentTargetData> targets =
                    await session.Query<DeploymentTargetData>()
                        .ToListAsync<DeploymentTargetData>(cancellationToken);

                IReadOnlyList<ProjectData> projects =
                    await session.Query<ProjectData>()
                        .ToListAsync<ProjectData>(cancellationToken);

                IReadOnlyList<OrganizationData> organizations =
                    await session.Query<OrganizationData>()
                        .ToListAsync<OrganizationData>(
                            cancellationToken);

                ImmutableArray<OrganizationInfo> deploymentTarget =
                    MapDataToOrganizations(organizations, projects, targets);

                return deploymentTarget;
            }
        }

        public async Task CreateOrganizationAsync(
            CreateOrganization createOrganization,
            CancellationToken cancellationToken)
        {
            using (IDocumentSession session = _documentStore.OpenSession())
            {
                var data = new OrganizationData
                {
                    Id = createOrganization.Id
                };

                session.Store(data);

                await session.SaveChangesAsync(cancellationToken);
            }

            _logger.Information("Created organization with id {Id}", createOrganization.Id);
        }

        public async Task CreateProjectAsync(CreateProject createProject, CancellationToken cancellationToken)
        {
            using (IDocumentSession session = _documentStore.OpenSession())
            {
                var data = new ProjectData
                {
                    Id = createProject.Id,
                    OrganizationId = createProject.OrganizationId
                };

                session.Store(data);

                await session.SaveChangesAsync(cancellationToken);
            }

            _logger.Information("Created project with id {Id}", createProject.Id);
        }

        private static DeploymentTarget MapDataToTarget(DeploymentTargetData deploymentTargetData)
        {
            var deploymentTargetAsync = new DeploymentTarget(
                deploymentTargetData.Id,
                deploymentTargetData.Name,
                "",
                deploymentTargetData.AllowExplicitPreRelease,
                deploymentTargetData.AllowedPackageNames.ToArray());

            return deploymentTargetAsync;
        }

        private ImmutableArray<OrganizationInfo> MapDataToOrganizations(
            IReadOnlyList<OrganizationData> organizations,
            IReadOnlyList<ProjectData> projects,
            IReadOnlyList<DeploymentTargetData> targets)
        {
            return organizations.Select(org => new OrganizationInfo(org.Id,
                    projects
                        .Where(project => project.OrganizationId.Equals(org.Id))
                        .Select(project =>
                            new ProjectInfo(org.Id,
                                project.Id,
                                targets
                                    .Where(target =>
                                        target.ProjectId.Equals(project.Id, StringComparison.OrdinalIgnoreCase))
                                    .Select(MapDataToTarget)))
                        .ToImmutableArray()))
                .ToImmutableArray();
        }
    }
}