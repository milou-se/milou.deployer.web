using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Schema.Validators;
using JetBrains.Annotations;
using Marten;
using MediatR;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Email;
using Milou.Deployer.Web.Core.Structure;
using Serilog;

namespace Milou.Deployer.Web.Core.Targets
{
    [UsedImplicitly]
    public partial class MartenStore : IDeploymentTargetReadService,
        IRequestHandler<CreateOrganization, CreateOrganizationResult>,
        IRequestHandler<CreateProject, CreateProjectResult>,
        IRequestHandler<CreateTarget, CreateTargetResult>,
        IRequestHandler<UpdateDeploymentTarget, UpdateDeploymentTargetResult>,
        INotificationHandler<DeploymentMetadataLogNotification>,
        INotificationHandler<DeploymentFinishedNotification>
    {
        private readonly IDocumentStore _documentStore;
        private ILogger _logger;

        public MartenStore([NotNull] IDocumentStore documentStore, ILogger logger)
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
                    .SingleOrDefaultAsync(target =>
                            target.Id.Equals(deploymentTargetId, StringComparison.OrdinalIgnoreCase),
                        cancellationToken);

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

        public async Task<ImmutableArray<DeploymentTarget>> GetDeploymentTargetsAsync(CancellationToken stoppingToken)
        {
            using (IQuerySession session = _documentStore.QuerySession())
            {
                IReadOnlyList<DeploymentTargetData> targets = await session.Query<DeploymentTargetData>()
                    .ToListAsync<DeploymentTargetData>(stoppingToken);

                return targets.Select(MapDataToTarget).ToImmutableArray();
            }
        }

        public async Task<ImmutableArray<ProjectInfo>> GetProjectsAsync(
            string organizationId,
            CancellationToken cancellationToken = default)
        {
            using (IQuerySession session = _documentStore.QuerySession())
            {
                IReadOnlyList<ProjectData> projects =
                    await session.Query<ProjectData>().Where(project =>
                            project.OrganizationId.Equals(organizationId, StringComparison.OrdinalIgnoreCase))
                        .ToListAsync(cancellationToken);

                return projects.Select(project =>
                        new ProjectInfo(project.OrganizationId, project.Id, ImmutableArray<DeploymentTarget>.Empty))
                    .ToImmutableArray();
            }
        }

        public async Task<CreateOrganizationResult> Handle(
            CreateOrganization request,
            CancellationToken cancellationToken)
        {
            CreateOrganizationResult result = await CreateOrganizationAsync(request, cancellationToken);

            return result;
        }

        public Task<CreateProjectResult> Handle(CreateProject request, CancellationToken cancellationToken)
        {
            return CreateProjectAsync(request, cancellationToken);
        }

        public async Task<CreateTargetResult> Handle(CreateTarget request, CancellationToken cancellationToken)
        {
            if (!request.IsValid)
            {
                return new CreateTargetResult(new ValidationError("Invalid"));
            }

            using (IDocumentSession session = _documentStore.OpenSession())
            {
                var data = new DeploymentTargetData
                {
                    Id = request.Id,
                    Name = request.Name
                };

                session.Store(data);

                await session.SaveChangesAsync(cancellationToken);
            }

            _logger.Information("Created target with id {Id}", request.Id);

            return new CreateTargetResult(request.Id);
        }

        public async Task<UpdateDeploymentTargetResult> Handle(
            UpdateDeploymentTarget request,
            CancellationToken cancellationToken)
        {
            using (IDocumentSession session = _documentStore.OpenSession())
            {
                DeploymentTargetData data =
                    await session.LoadAsync<DeploymentTargetData>(request.Id, cancellationToken);

                if (data is null)
                {
                    return new UpdateDeploymentTargetResult(new ValidationError("Not found"));
                }

                data.PackageId = request.PackageId;
                data.Url = request.Url;
                data.IisSiteName = request.IisSiteName;
                data.AllowExplicitPreRelease = request.AllowExplicitPreRelease;
                data.NuGetPackageSource = request.NugetPackageSource;
                data.NuGetConfigFile = request.NugetConfigFile;
                data.AutoDeployEnabled = request.AutoDeployEnabled;
                session.Store(data);

                await session.SaveChangesAsync(cancellationToken);
            }

            _logger.Information("Updated target with id {Id}", request.Id);

            return new UpdateDeploymentTargetResult();
        }

        public async Task<CreateProjectResult> CreateProjectAsync(
            CreateProject createProject,
            CancellationToken cancellationToken)
        {
            if (!createProject.IsValid)
            {
                return new CreateProjectResult(new ValidationError("Id or organization id is invalid"));
            }

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

            return new CreateProjectResult(createProject.Id);
        }

        private static DeploymentTarget MapDataToTarget(DeploymentTargetData deploymentTargetData)
        {
            if (deploymentTargetData is null)
            {
                return null;
            }

            var deploymentTargetAsync = new DeploymentTarget(
                deploymentTargetData.Id,
                deploymentTargetData.Name,
                deploymentTargetData.PackageId ?? "N/A",
                deploymentTargetData.AllowExplicitPreRelease,
                uri: deploymentTargetData.Url?.ToString(),
                nuGetConfigFile: deploymentTargetData.NuGetConfigFile,
                nuGetPackageSource: deploymentTargetData.NuGetPackageSource,
                iisSiteName: deploymentTargetData.IisSiteName,
                autoDeployEnabled: deploymentTargetData.AutoDeployEnabled);

            return deploymentTargetAsync;
        }

        private async Task<CreateOrganizationResult> CreateOrganizationAsync(
            CreateOrganization createOrganization,
            CancellationToken cancellationToken)
        {
            if (!createOrganization.IsValid)
            {
                return new CreateOrganizationResult(new ValidationError("Missing ID"));
            }

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

            return new CreateOrganizationResult();
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
                                        target.ProjectId != null &&
                                        target.ProjectId.Equals(project.Id, StringComparison.OrdinalIgnoreCase))
                                    .Select(MapDataToTarget)))
                        .ToImmutableArray()))
                .Concat(new[]
                {
                    new OrganizationInfo("NA",
                        new[]
                        {
                            new ProjectInfo(
                                "NA",
                                "NA",
                                targets
                                    .Where(target => target.ProjectId is null)
                                    .Select(MapDataToTarget))
                        })
                })
                .ToImmutableArray();
        }

        public async Task Handle(DeploymentMetadataLogNotification notification, CancellationToken cancellationToken)
        {
            using (IDocumentSession session = _documentStore.OpenSession())
            {
                var taskMetadata = new TaskMetadata
                {
                    DeploymentTaskId = notification.DeploymentTask.DeploymentTaskId,
                    DeploymentTargetId = notification.DeploymentTask.DeploymentTargetId,
                    Id = $"deploymentTaskMetadata/{notification.DeploymentTask.DeploymentTaskId}",
                    Metadata = notification.MetadataContent
                };


                session.Store(taskMetadata);

                await session.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task Handle(DeploymentFinishedNotification notification, CancellationToken cancellationToken)
        {
            using (IDocumentSession session = _documentStore.OpenSession())
            {
                var taskMetadata = new TaskLog
                {
                    DeploymentTaskId = notification.DeploymentTask.DeploymentTaskId,
                    DeploymentTargetId = notification.DeploymentTask.DeploymentTargetId,
                    Id = $"deploymentTaskLog/{notification.DeploymentTask.DeploymentTaskId}",
                    Log = notification.Log
                };

                session.Store(taskMetadata);

                await session.SaveChangesAsync(cancellationToken);
            }

        }
    }
}