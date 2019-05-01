using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.DependencyInjection;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class DataSeederTestModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder)
        {
            return builder.RegisterAssemblyTypesAsSingletons<IDataSeeder>(ApplicationAssemblies.FilteredAssemblies());
        }
    }

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
                url: Environment.GetEnvironmentVariable("TestDeploymentUri").ParseUriOrDefault(),
                emailNotificationAddresses: new StringValues("noreply@localhost.local"),
                enabled: true);

            var createTarget = new CreateTarget(testTarget.Id, testTarget.Name);
            await _mediator.Send(createTarget, cancellationToken);

            var updateDeploymentTarget = new UpdateDeploymentTarget(testTarget.Id,
                testTarget.AllowPreRelease,
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
