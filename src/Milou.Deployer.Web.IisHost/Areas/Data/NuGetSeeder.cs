using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using MediatR;

using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.Targets;

using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Data
{
    [UsedImplicitly]
    public class NuGetSeeder : IDataSeeder
    {
        private readonly IMediator _mediator;

        private readonly IDeploymentTargetReadService _deploymentTargetReadService;

        private readonly ILogger _logger;

        public NuGetSeeder(IMediator mediator, IDeploymentTargetReadService deploymentTargetReadService, ILogger logger)
        {
            _mediator = mediator;
            _deploymentTargetReadService = deploymentTargetReadService;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            try
            {
                var targets =
                    await _deploymentTargetReadService.GetDeploymentTargetsAsync(stoppingToken: cancellationToken);
                foreach (var deploymentTarget in targets)
                {
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
                        deploymentTarget.EnvironmentType?.Name,
                        deploymentTarget.NuGet.PackageListTimeout?.ToString(),
                        deploymentTarget.PublishType?.ToString(),
                        deploymentTarget.FtpPath?.Path);

                    await _mediator.Send(updateDeploymentTarget, cancellationToken);
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.Error(ex, "Could not run seeder task");
            }
        }
    }
}