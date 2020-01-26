using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Environments;
using Milou.Deployer.Web.Core.Deployment.Targets;

namespace Milou.Deployer.Web.IisHost.Areas.Data
{
    [UsedImplicitly]
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