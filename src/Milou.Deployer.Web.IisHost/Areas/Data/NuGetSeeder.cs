using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Data
{
    [UsedImplicitly]
    public class NuGetSeeder : IDataSeeder
    {
        private readonly IDeploymentTargetReadService _deploymentTargetReadService;

        private readonly ILogger _logger;
        private readonly IEnvironmentTypeService _environmentTypeService;
        private readonly IMediator _mediator;

        public NuGetSeeder(
            IMediator mediator,
            IDeploymentTargetReadService deploymentTargetReadService,
            ILogger logger,
            IEnvironmentTypeService environmentTypeService)
        {
            _mediator = mediator;
            _deploymentTargetReadService = deploymentTargetReadService;
            _logger = logger;
            _environmentTypeService = environmentTypeService;
        }

        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            try
            {
                var environmentTypes = await _environmentTypeService.GetEnvironmentTypes(cancellationToken);

                var targets =
                    await _deploymentTargetReadService.GetDeploymentTargetsAsync(stoppingToken: cancellationToken);

                foreach (var deploymentTarget in targets)
                {
                    string? typeId = default;
                    string environmentConfiguration = deploymentTarget.EnvironmentConfiguration;

                    if (string.IsNullOrWhiteSpace(deploymentTarget.EnvironmentTypeId)
                        && !string.IsNullOrWhiteSpace(environmentConfiguration))
                    {
                        string configuration = environmentConfiguration;

                        var foundType = environmentTypes.SingleOrDefault(type =>
                            type.Name.Equals(configuration,
                                StringComparison.OrdinalIgnoreCase));

                        if (foundType is { })
                        {
                            typeId = foundType.Id;
                            environmentConfiguration = null;
                        }
                    }

                    var updateDeploymentTarget = new UpdateDeploymentTarget(
                        deploymentTarget.Id,
                        deploymentTarget.AllowExplicitExplicitPreRelease ?? false,
                        deploymentTarget.Url.ToString(),
                        deploymentTarget.PackageId,
                        deploymentTarget.IisSiteName,
                        deploymentTarget.NuGet.NuGetPackageSource,
                        deploymentTarget.NuGet.NuGetConfigFile,
                        deploymentTarget.AutoDeployEnabled,
                        deploymentTarget.PublishSettingsXml,
                        deploymentTarget.TargetDirectory,
                        deploymentTarget.WebConfigTransform,
                        deploymentTarget.ExcludedFilePatterns,
                        deploymentTarget.EnvironmentTypeId ?? typeId,
                        deploymentTarget.NuGet.PackageListTimeout?.ToString(),
                        deploymentTarget.PublishType?.ToString(),
                        deploymentTarget.FtpPath?.Path,
                        deploymentTarget.MetadataTimeout?.ToString(),
                        deploymentTarget.RequireEnvironmentConfiguration ?? false,
                        environmentConfiguration);

                    await _mediator.Send(updateDeploymentTarget, cancellationToken);
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.Error(ex, "Could not run seeder task");
            }
        }

        public int Order => int.MaxValue;
    }
}