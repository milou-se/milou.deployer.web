using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Environments;
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
        private readonly IMediator _mediator;

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
                        deploymentTarget.NuGetPackageSource,
                        deploymentTarget.NuGetConfigFile,
                        deploymentTarget.AutoDeployEnabled,
                        deploymentTarget.PublishSettingsXml,
                        deploymentTarget.TargetDirectory,
                        deploymentTarget.WebConfigTransform,
                        deploymentTarget.ExcludedFilePatterns,
                        deploymentTarget.EnvironmentTypeId,
                        deploymentTarget.PackageListTimeout?.ToString(),
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

    public class EnvironmentTypeSeeder : IDataSeeder
    {
        private readonly IEnvironmentTypeService _environmentTypeService;
        private readonly IMediator _mediator;

        public EnvironmentTypeSeeder(IMediator mediator, IEnvironmentTypeService environmentTypeService)
        {
            _mediator = mediator;
            _environmentTypeService = environmentTypeService;
        }

        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            var types = await _environmentTypeService.GetEnvironmentTypes(cancellationToken);

            if (!types.IsDefaultOrEmpty)
            {
                return;
            }

            var commands = new[]
            {
                new CreateEnvironment
                {
                    EnvironmentTypeId = "QA",
                    EnvironmentTypeName = "QualityAssurance",
                    PreReleaseBehavior = PreReleaseBehavior.AllowWithForceFlag.Name
                },
                new CreateEnvironment
                {
                    EnvironmentTypeId = "Production",
                    EnvironmentTypeName = "Production",
                    PreReleaseBehavior = PreReleaseBehavior.Deny.Name
                },
                new CreateEnvironment
                {
                    EnvironmentTypeId = "Development ",
                    EnvironmentTypeName = "Development ",
                    PreReleaseBehavior = PreReleaseBehavior.Allow.Name
                },
                new CreateEnvironment
                {
                    EnvironmentTypeId = "Test",
                    EnvironmentTypeName = "Test ",
                    PreReleaseBehavior = PreReleaseBehavior.Allow.Name
                }
            };

            foreach (var createEnvironment in commands)
            {
                await _mediator.Send(createEnvironment, cancellationToken);
            }
        }
    }
}