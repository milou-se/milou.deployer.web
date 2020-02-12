using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Environments;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Data
{
    [UsedImplicitly]
    public class EnvironmentTypeSeeder : IDataSeeder
    {
        private readonly IEnvironmentTypeService _environmentTypeService;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public EnvironmentTypeSeeder(IMediator mediator, IEnvironmentTypeService environmentTypeService, ILogger logger)
        {
            _mediator = mediator;
            _environmentTypeService = environmentTypeService;
            _logger = logger;
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
                    EnvironmentTypeId = "qa",
                    EnvironmentTypeName = "QualityAssurance",
                    PreReleaseBehavior = PreReleaseBehavior.AllowWithForceFlag.Name
                },
                new CreateEnvironment
                {
                    EnvironmentTypeId = "production",
                    EnvironmentTypeName = "Production",
                    PreReleaseBehavior = PreReleaseBehavior.Deny.Name
                },
                new CreateEnvironment
                {
                    EnvironmentTypeId = "development ",
                    EnvironmentTypeName = "Development ",
                    PreReleaseBehavior = PreReleaseBehavior.Allow.Name
                },
                new CreateEnvironment
                {
                    EnvironmentTypeId = "test",
                    EnvironmentTypeName = "Test ",
                    PreReleaseBehavior = PreReleaseBehavior.Allow.Name
                }
            };

            foreach (var createEnvironment in commands)
            {
                var result = await _mediator.Send(createEnvironment, cancellationToken);

                _logger.Debug("CreateEnvironment result for Id {Id}: {Status}", result.Id, result.Status);
            }
        }
    }
}