
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Marten;
using MediatR;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Newtonsoft.Json;
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
        IRequestHandler<DeploymentLogRequest, DeploymentLogResponse>,
        IRequestHandler<RemoveTarget, Unit>,
        IRequestHandler<EnableTarget, Unit>,
        IRequestHandler<DisableTarget, Unit>
    {
        private readonly IDocumentStore _documentStore;
        private readonly ILogger _logger;

        private readonly IMediator _mediator;

        public MartenStore([NotNull] IDocumentStore documentStore, ILogger logger, IMediator mediator)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            _logger = logger;
            _mediator = mediator;
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

        private DeploymentTarget MapDataToTarget(DeploymentTargetData deploymentTargetData)
        {
            if (deploymentTargetData is null)
            {
                return null;
            }

            DeploymentTarget deploymentTargetAsync = null;
            try
            {
                deploymentTargetAsync = new DeploymentTarget(
                    deploymentTargetData.Id,
                    deploymentTargetData.Name,
                    deploymentTargetData.PackageId.WithDefault(Constants.NotAvailable),
                    deploymentTargetData.PublishSettingsXml,
                    deploymentTargetData.AllowExplicitPreRelease,
                    url: deploymentTargetData.Url,
                    nuGetConfigFile: deploymentTargetData.NuGetConfigFile,
                    nuGetPackageSource: deploymentTargetData.NuGetPackageSource,
                    iisSiteName: deploymentTargetData.IisSiteName,
                    autoDeployEnabled: deploymentTargetData.AutoDeployEnabled,
                    targetDirectory: deploymentTargetData.TargetDirectory,
                    webConfigTransform: deploymentTargetData.WebConfigTransform,
                    excludedFilePatterns: deploymentTargetData.ExcludedFilePatterns,
                    environmentType: deploymentTargetData.EnvironmentType,
                    enabled: deploymentTargetData.Enabled,
                    packageListTimeout: deploymentTargetData.PackageListTimeout,
                    publishType: deploymentTargetData.PublishType,
                    ftpPath: deploymentTargetData.FtpPath,
                    metadataTimeout: deploymentTargetData.MetadataTimeout,
                    nuget: MapNuGet(deploymentTargetData.NuGetData));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not get deployment target from data {Data}", JsonConvert.SerializeObject(deploymentTargetData));
            }

            return deploymentTargetAsync;
        }

        private TargetNuGetSettings MapNuGet(NuGetData nugetData) =>
            new TargetNuGetSettings
            {
                PackageListTimeout = nugetData?.PackageListTimeout,
                NuGetPackageSource = nugetData?.NuGetPackageSource,
                NuGetConfigFile = nugetData?.NuGetConfigFile
            };

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
                        .Where(project => project.OrganizationId.Equals(org.Id, StringComparison.OrdinalIgnoreCase))
                        .Select(project =>
                            new ProjectInfo(org.Id,
                                project.Id,
                                targets
                                    .Where(target =>
                                        target.ProjectId != null
                                        && target.ProjectId.Equals(project.Id, StringComparison.OrdinalIgnoreCase))
                                    .Select(MapDataToTarget)
                                    .Where(t => t != null)))
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
                                    .Select(MapDataToTarget)
                                    .Where(t => t != null))
                        })
                })
                .ToImmutableArray();
        }

        public Task<DeploymentTarget> GetDeploymentTargetAsync(
            [NotNull] string deploymentTargetId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(deploymentTargetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(deploymentTargetId));
            }

            return FindDeploymentTargetAsync(deploymentTargetId, cancellationToken);
        }

        private async Task<DeploymentTarget> FindDeploymentTargetAsync(string deploymentTargetId, CancellationToken cancellationToken)
        {
            using (var session = _documentStore.QuerySession())
            {
                try
                {
                    var deploymentTargetData = await session.Query<DeploymentTargetData>()
                        .SingleOrDefaultAsync(target =>
                                target.Id.Equals(deploymentTargetId, StringComparison.OrdinalIgnoreCase),
                            cancellationToken);

                    var deploymentTarget = MapDataToTarget(deploymentTargetData);

                    return deploymentTarget ?? DeploymentTarget.None;
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
                            .Where(target => target.Enabled)
                            .ToListAsync(cancellationToken);

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

        public async Task<ImmutableArray<DeploymentTarget>> GetDeploymentTargetsAsync(TargetOptions options = default, CancellationToken stoppingToken = default)
        {
            using (var session = _documentStore.QuerySession())
            {
                bool Filter(DeploymentTarget target)
                {
                    if (options is null || options.OnlyEnabled)
                    {
                        return target.Enabled;
                    }

                    return true;
                }

                try
                {
                    var targets = await session.Query<DeploymentTargetData>()
                        .ToListAsync<DeploymentTargetData>(stoppingToken);

                    var deploymentTargets = targets
                        .Select(MapDataToTarget)
                        .Where(Filter)
                        .ToImmutableArray();

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
            [NotNull] CreateOrganization request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var result = await CreateOrganizationAsync(request, cancellationToken);

            return result;
        }

        public Task<CreateProjectResult> Handle([NotNull] CreateProject request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return CreateProjectAsync(request, cancellationToken);
        }

        public async Task<CreateTargetResult> Handle([NotNull] CreateTarget createTarget, CancellationToken cancellationToken)
        {
            if (createTarget == null)
            {
                throw new ArgumentNullException(nameof(createTarget));
            }

            if (!createTarget.IsValid)
            {
                return new CreateTargetResult(new ValidationError("Invalid"));
            }

            using (var session = _documentStore.OpenSession())
            {
                var data = new DeploymentTargetData
                {
                    Id = createTarget.Id,
                    Name = createTarget.Name
                };

                session.Store(data);

                await session.SaveChangesAsync(cancellationToken);
            }

            _logger.Information("Created target with id {Id}", createTarget.Id);

            return new CreateTargetResult(createTarget.Id, createTarget.Name);
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
            IReadOnlyCollection<LogItem> taskLog;

            string id = $"deploymentTaskLog/{request.DeploymentTaskId}";

            int level = (int)request.Level;

            using (var session = _documentStore.LightweightSession())
            {
                taskLog = await session.Query<LogItem>()
                    .Where(log => log.TaskLogId == id && log.Level >= level)
                    .ToListAsync(cancellationToken);
            }

            if (taskLog is null)
            {
                return new DeploymentLogResponse(Array.Empty<LogItem>());
            }

            return new DeploymentLogResponse(taskLog);
        }

        public async Task<UpdateDeploymentTargetResult> Handle(
            [NotNull] UpdateDeploymentTarget request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string targetName;

            string id;

            using (var session = _documentStore.OpenSession())
            {
                var data =
                    await session.LoadAsync<DeploymentTargetData>(request.Id, cancellationToken);

                if (data is null)
                {
                    return new UpdateDeploymentTargetResult("", "", new ValidationError("Not found"));
                }

                id = data.Id;
                targetName = data.Name;

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
                data.FtpPath = request.FtpPath?.Path;
                data.PublishType = request.PublishType.Name;
                data.EnvironmentType = request.EnvironmentType.Name;
                data.PackageListTimeout = request.PackageListTimeout;
                data.NuGetData ??= new NuGetData
                                   {
                                       NuGetConfigFile = request.NugetConfigFile,
                                       NuGetPackageSource = request.NugetPackageSource,
                                       PackageListTimeout = request.PackageListTimeout
                                   };
                data.MetadataTimeout = request.MetadataTimeout;
                session.Store(data);

                await session.SaveChangesAsync(cancellationToken);
            }

            _logger.Information("Updated target with id {Id}", request.Id);

            var updateDeploymentTargetResult = new UpdateDeploymentTargetResult(targetName, id);

            await _mediator.Publish(updateDeploymentTargetResult, cancellationToken);

            return updateDeploymentTargetResult;
        }

        public async Task<Unit> Handle([NotNull] RemoveTarget request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (var session = _documentStore.OpenSession())
            {
                session.Delete<DeploymentTargetData>(request.TargetId);
                session.DeleteWhere<TaskMetadata>(m => m.DeploymentTargetId.Equals(request.TargetId, StringComparison.OrdinalIgnoreCase));

                await session.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }

        public async Task<Unit> Handle(EnableTarget request, CancellationToken cancellationToken)
        {
            using (var session = _documentStore.OpenSession())
            {
                var deploymentTargetData = await session.LoadAsync<DeploymentTargetData>(request.TargetId, cancellationToken);

                if (deploymentTargetData is null)
                {
                    return Unit.Value;
                }

                deploymentTargetData.Enabled = true;

                session.Store(deploymentTargetData);

                await session.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }

        public async Task<Unit> Handle([NotNull] DisableTarget request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (var session = _documentStore.OpenSession())
            {
                var deploymentTargetData = await session.LoadAsync<DeploymentTargetData>(request.TargetId, cancellationToken);

                if (deploymentTargetData is null)
                {
                    return Unit.Value;
                }

                deploymentTargetData.Enabled = false;

                session.Store(deploymentTargetData);

                await session.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}
