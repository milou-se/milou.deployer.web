using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.JsonConfiguration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Structure;
using Serilog;

namespace Milou.Deployer.Web.Core.Targets
{
    [UsedImplicitly]
    public class JsonTargetSource
    {
        private readonly ILogger _logger;
        private readonly JsonDeploymentTargetSourceConfiguration _configuration;
        private EnvironmentConfiguration _environment;

        public JsonTargetSource(
            [NotNull] EnvironmentConfiguration environment,
            [NotNull] ILogger logger,
            [NotNull] JsonDeploymentTargetSourceConfiguration configuration)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task<IReadOnlyCollection<OrganizationInfo>> GetTargetsAsync(CancellationToken cancellationToken)
        {
            string jsonTargetsFile = _configuration.SourceFile.WithDefault(Path.Combine(_environment.ApplicationBasePath, "targets.json"));

            _logger.Information("Reading targets from JSON file '{JsonFile}'", jsonTargetsFile);

            IKeyValueConfiguration keyValueConfiguration = new JsonKeyValueConfiguration(jsonTargetsFile);

            IReadOnlyCollection<DeploymentTarget> targets = keyValueConfiguration.GetInstances<DeploymentTarget>();

            IReadOnlyCollection<OrganizationInfo> organizations =
                targets.GroupBy(target => target.Organization.WithDefault("Global"))
                    .Select(
                        organizationGroup => new
                        {
                            Organization = organizationGroup.Key,
                            Projects = organizationGroup
                                .GroupBy(target => target.ProjectInvariantName.WithDefault(Constants.NotAvailable))
                                .Select(
                                    projectGroup => new ProjectInfo(organizationGroup.Key,
                                        projectGroup.Key,
                                        projectGroup.OrderBy(target => target.Name)))
                        })
                    .Select(
                        organizationGroup =>
                            new OrganizationInfo(organizationGroup.Organization,
                                organizationGroup.Projects.OrderBy(project => project.ProjectDisplayName)))
                    .OrderBy(organization => organization.Organization)
                    .SafeToReadOnlyCollection();

            return Task.FromResult(organizations);
        }
    }
}