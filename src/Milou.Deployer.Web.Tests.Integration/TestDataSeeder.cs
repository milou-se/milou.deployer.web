using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Primitives;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Targets;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class TestDataSeeder : IDataSeeder
    {
        private readonly IMediator _mediator;

        public TestDataSeeder(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            var testTarget = new DeploymentTarget("TestTarget",
                "Test target",
                "MilouDeployerWebTest",
                allowExplicitPreRelease: false,
                autoDeployEnabled: true,
                targetDirectory: Environment.GetEnvironmentVariable("TestDeploymentTargetPath"),
                uri: Environment.GetEnvironmentVariable("TestDeploymentUri"),
                emailNotificationAddresses: new StringValues("noreply@localhost.local"),
                enabled: true);

            var createTarget = new CreateTarget(testTarget.Id, testTarget.Name);
            await _mediator.Send(createTarget, cancellationToken);

            var updateDeploymentTarget = new UpdateDeploymentTarget(testTarget.Id,
                testTarget.AllowPrerelease,
                testTarget.Url,
                testTarget.PackageId,
                autoDeployEnabled: testTarget.AutoDeployEnabled,
                targetDirectory: testTarget.TargetDirectory,
                enabled: true);

            await _mediator.Send(updateDeploymentTarget,
                cancellationToken);
        }
    }
}