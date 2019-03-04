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
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Targets;
using Serilog;

namespace Milou.Deployer.Web.Marten
{
    [UsedImplicitly]
    public class MartenStore : IDeploymentTargetReadService,
        IRequestHandler<CreateOrganization, CreateOrganizationResult>,
        IRequestHandler<CreateProject, CreateProjectResult>,
        IRequestHandler<CreateTarget, CreateTargetResult>,
        IRequestHandler<UpdateDeploymentTarget, UpdateDeploymentTargetResult>,
        IRequestHandler<DeploymentHistoryRequest, DeploymentHistoryResponse>,
        IRequestHandler<DeploymentLogRequest, DeploymentLogResponse>
    {
        private readonly IDocumentStore _documentStore;
        private readonly ILogger _logger;

        public MartenStore([NotNull] IDocumentStore documentStore, ILogger logger)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            _logger = logger;
        }

        private async Task<CreateProjectResult> CreateProjectAsync(
            CreateProject createProject,
            CancellationToken cancellationToken)
        {
            if (!createProject.IsValid)
            {
                return new CreateProjectResult(new ValidationError("Id or organization id is invalid"));
            }

            using (var session = _documentStore.OpenSession())
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
                deploymentTargetData.PackageId ?? Constants.NotAvailable,
                deploymentTargetData.PublishSettingsXml,
                deploymentTargetData.AllowExplicitPreRelease,
                uri: deploymentTargetData.Url?.ToString(),
                nuGetConfigFile: deploymentTargetData.NuGetConfigFile,
                nuGetPackageSource: deploymentTargetData.NuGetPackageSource,
                iisSiteName: deploymentTargetData.IisSiteName,
                autoDeployEnabled: deploymentTargetData.AutoDeployEnabled,
                targetDirectory: deploymentTargetData.TargetDirectory,
                webConfigTransform: deploymentTargetData.WebConfigTransform,
                excludedFilePatterns: deploymentTargetData.ExcludedFilePatterns,
                enabled: deploymentTargetData.Enabled);

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

            using (var session = _documentStore.OpenSession())
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
                                        target.ProjectId != null
                                        && target.ProjectId.Equals(project.Id, StringComparison.OrdinalIgnoreCase))
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

        public async Task<DeploymentTarget> GetDeploymentTargetAsync(
            [NotNull] string deploymentTargetId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(deploymentTargetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(deploymentTargetId));
            }

            using (var session = _documentStore.QuerySession())
            {
                try
                {
                    var deploymentTargetData = await session.Query<DeploymentTargetData>()
                        .SingleOrDefaultAsync(target =>
                                target.Id.Equals(deploymentTargetId, StringComparison.OrdinalIgnoreCase),
                            cancellationToken);

                    var deploymentTarget = MapDataToTarget(deploymentTargetData);

                    return deploymentTarget;
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.Warning(ex, "Could not get deployment target with id {Id}", deploymentTargetId);
                    return DeploymentTarget.None;
                }
            }
        }

        public async Task<ImmutableArray<OrganizationInfo>> GetOrganizationsAsync(
            CancellationToken cancellationToken = default)
        {
            using (var session = _documentStore.QuerySession())
            {
                try
                {
                    var targets =
                        await session.Query<DeploymentTargetData>()
                            .ToListAsync<DeploymentTargetData>(cancellationToken);

                    var projects =
                        await session.Query<ProjectData>()
                            .ToListAsync<ProjectData>(cancellationToken);

                    var organizations =
                        await session.Query<OrganizationData>()
                            .ToListAsync<OrganizationData>(
                                cancellationToken);

                    var organizationsInfo =
                        MapDataToOrganizations(organizations, projects, targets);

                    return organizationsInfo;
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.Warning(ex, "Could not get any organizations targets");
                    return ImmutableArray<OrganizationInfo>.Empty;
                }
            }
        }

        public async Task<ImmutableArray<DeploymentTarget>> GetDeploymentTargetsAsync(CancellationToken stoppingToken)
        {
            using (var session = _documentStore.QuerySession())
            {
                try
                {
                    var targets = await session.Query<DeploymentTargetData>()
                        .ToListAsync<DeploymentTargetData>(stoppingToken);

                    var deploymentTargets =
                        targets.Select(MapDataToTarget).ToImmutableArray();

                    return deploymentTargets;
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.Warning(ex, "Could not get any deployment targets");
                    return ImmutableArray<DeploymentTarget>.Empty;
                }
            }
        }

        public async Task<ImmutableArray<ProjectInfo>> GetProjectsAsync(
            string organizationId,
            CancellationToken cancellationToken = default)
        {
            using (var session = _documentStore.QuerySession())
            {
                var projects =
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
            var result = await CreateOrganizationAsync(request, cancellationToken);

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

            using (var session = _documentStore.OpenSession())
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

        public async Task<DeploymentHistoryResponse> Handle(
            DeploymentHistoryRequest request,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<TaskMetadata> taskMetadata;
            using (var session = _documentStore.LightweightSession())
            {
                taskMetadata = await session.Query<TaskMetadata>()
                    .Where(item =>
                        item.DeploymentTargetId.Equals(request.DeploymentTargetId, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(item => item.FinishedAtUtc)
                    .ToListAsync(cancellationToken);
            }

            return new DeploymentHistoryResponse(taskMetadata
                .Select(item =>
                    new DeploymentTaskInfo(
                        item.DeploymentTaskId,
                        item.Metadata,
                        item.StartedAtUtc,
                        item.FinishedAtUtc,
                        item.ExitCode,
                        item.PackageId,
                        item.Version))
                .ToImmutableArray());
        }

        public async Task<DeploymentLogResponse> Handle(
            DeploymentLogRequest request,
            CancellationToken cancellationToken)
        {
            TaskLog taskLog;

            var id = $"deploymentTaskLog/{request.DeploymentTaskId}";

            using (var session = _documentStore.LightweightSession())
            {
                taskLog = await session.LoadAsync<TaskLog>(id, cancellationToken);
            }

            if (taskLog is null)
            {
                return new DeploymentLogResponse(string.Empty);
            }

            return new DeploymentLogResponse(taskLog.Log);
        }

        public async Task<UpdateDeploymentTargetResult> Handle(
            UpdateDeploymentTarget request,
            CancellationToken cancellationToken)
        {
            using (var session = _documentStore.OpenSession())
            {
                var data =
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
                data.PublishSettingsXml = request.PublishSettingsXml;
                data.TargetDirectory = request.TargetDirectory;
                data.WebConfigTransform = request.WebConfigTransform;
                data.ExcludedFilePatterns = request.ExcludedFilePatterns;
                data.Enabled = request.Enabled;
                session.Store(data);

                await session.SaveChangesAsync(cancellationToken);
            }

            _logger.Information("Updated target with id {Id}", request.Id);

            return new UpdateDeploymentTargetResult();
        }
    }
}
