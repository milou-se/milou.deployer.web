using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Newtonsoft.Json;
using Serilog;

namespace Milou.Deployer.Web.Core.Targets
{
    [UsedImplicitly]
    public class JsonTargetSource : IDeploymentTargetReadService,
        IRequestHandler<CreateOrganization, CreateOrganizationResult>,
        IRequestHandler<CreateProject, CreateProjectResult>,
        IRequestHandler<CreateTarget, CreateTargetResult>,
        IRequestHandler<UpdateDeploymentTarget, UpdateDeploymentTargetResult>,
        IRequestHandler<DeploymentHistoryRequest, DeploymentHistoryResponse>,
        IRequestHandler<DeploymentLogRequest, DeploymentLogResponse>
    {
        private readonly JsonDeploymentTargetSourceConfiguration _configuration;
        private readonly EnvironmentConfiguration _environment;
        private readonly ILogger _logger;

        public JsonTargetSource(
            [NotNull] EnvironmentConfiguration environment,
            [NotNull] ILogger logger,
            [NotNull] JsonDeploymentTargetSourceConfiguration configuration)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<ImmutableArray<OrganizationInfo>> GetTargetsAsync(CancellationToken cancellationToken)
        {
            var jsonTargetsFile = JsonTargetsFile();

            _logger.Information("Reading targets from JSON file '{JsonFile}'", jsonTargetsFile);

            var organizations = JsonConvert.DeserializeObject<OrganizationInfo[]>(await File.ReadAllTextAsync(
                jsonTargetsFile,
                Encoding.UTF8,
                cancellationToken));

           return organizations.ToImmutableArray();
        }

        private string JsonTargetsFile()
        {
            var jsonTargetsFile =
                _configuration.SourceFile.WithDefault(Path.Combine(_environment.ApplicationBasePath, "targets.json"));
            return jsonTargetsFile;
        }

        public async Task<DeploymentTarget> GetDeploymentTargetAsync(string deploymentTargetId, CancellationToken cancellationToken = default)
        {
            return (await GetDeploymentTargetsAsync(cancellationToken)).SingleOrDefault(s =>
                s.Id.Equals(deploymentTargetId, StringComparison.OrdinalIgnoreCase));
        }

        public Task<ImmutableArray<OrganizationInfo>> GetOrganizationsAsync(CancellationToken cancellationToken = default)
        {
            return GetTargetsAsync(cancellationToken);
        }

        public async Task<ImmutableArray<DeploymentTarget>> GetDeploymentTargetsAsync(CancellationToken stoppingToken)
        {
            var organizationInfos = await GetTargetsAsync(stoppingToken);

            return organizationInfos.SelectMany(o => o.Projects.SelectMany(p => p.DeploymentTargets))
                .ToImmutableArray();
        }

        public async Task<ImmutableArray<ProjectInfo>> GetProjectsAsync(string organizationId, CancellationToken cancellationToken = default)
        {
            var organizationInfos = await GetTargetsAsync(cancellationToken);
            return organizationInfos
                .Where(o => o.Organization.Equals(organizationId, StringComparison.OrdinalIgnoreCase))
                .SelectMany(o => o.Projects)
                .ToImmutableArray();
        }

        public Task<CreateOrganizationResult> Handle(CreateOrganization request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CreateProjectResult> Handle(CreateProject request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CreateTargetResult> Handle(CreateTarget request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateDeploymentTargetResult> Handle(UpdateDeploymentTarget request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<DeploymentHistoryResponse> Handle(DeploymentHistoryRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<DeploymentLogResponse> Handle(DeploymentLogRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
