using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Schema.Json;
using Autofac;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Primitives;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Targets;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.Tests.Integration.TestData;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class AutoDeploySetup : WebFixtureBase, IAppHost
    {
        private IWebHost _webHost;

        protected TestConfiguration TestConfiguration;

        public HttpResponseMessage ResponseMessage { get; private set; }

        public PortPoolRental TestSiteHttpPort { get; private set; }

        protected override async Task RunAsync()
        {
            using (var httpClient = new HttpClient())
            {
                ResponseMessage =
                    await httpClient.GetAsync($"http://localhost:{TestSiteHttpPort.Port}/applicationmetadata.json");
            }
        }

        protected override async Task BeforeInitialize(CancellationToken cancellationToken)
        {
            var portPoolRange = new PortPoolRange(5200, 5299);
            TestSiteHttpPort = TcpHelper.GetAvailablePort(portPoolRange);

            TestConfiguration = await new TestPathHelper().CreateTestConfigurationAsync(cancellationToken);

            Environment.SetEnvironmentVariable("TestDeploymentTargetPath", TestConfiguration.SiteAppRoot.FullName);
            Environment.SetEnvironmentVariable("TestDeploymentUri", $"http://localhost:{TestSiteHttpPort.Port}");
            string nugetExePath = Path.Combine(VcsTestPathHelper.GetRootDirectory(), "tools", "nuget", "nuget.exe");
            Environment.SetEnvironmentVariable(ConfigurationConstants.NuGetExePath, nugetExePath);

            string deployerDir = Path.Combine(VcsTestPathHelper.GetRootDirectory(), "tools", "milou.deployer");

            ImmutableArray<KeyValue> keys = new List<KeyValue>
            {
                new KeyValue("urn:milou-deployer:tools:nuget:source", "Milou.Deployer.Web.Tests.Integration", null),
                new KeyValue("urn:milou-deployer:tools:nuget:config", TestConfiguration.NugetConfigFile.FullName, null),
                new KeyValue("tools:nuget:config-file", TestConfiguration.NugetConfigFile.FullName, null),
                new KeyValue("urn:milou-deployer:tools:nuget:exe-path", nugetExePath, null)
            }.ToImmutableArray();

            var jsonConfigurationSerializer = new JsonConfigurationSerializer();
            string serialized = jsonConfigurationSerializer.Serialize(new ConfigurationItems("1.0", keys));

            string settingsFile = Path.Combine(deployerDir, $"{Environment.MachineName}.settings.json");

            FilesToClean.Add(new FileInfo(settingsFile));

            File.WriteAllText(settingsFile, serialized, Encoding.UTF8);

            FileInfo[] nugetPackages = new DirectoryInfo(Path.Combine(VcsTestPathHelper.GetRootDirectory(),
                "src",
                "Milou.Deployer.Web.Tests.Integration")).GetFiles("*.nupkg");

            foreach (FileInfo nugetPackage in nugetPackages)
            {
                nugetPackage.CopyTo(Path.Combine(TestConfiguration.NugetPackageDirectory.FullName, nugetPackage.Name));
            }

            Environment.SetEnvironmentVariable(ConfigurationConstants.DeployerExePath,
                Path.Combine(deployerDir, "Milou.Deployer.ConsoleClient.exe"));
            Environment.SetEnvironmentVariable(ConfigurationConstants.NugetConfigFile,
                TestConfiguration.NugetConfigFile.FullName);
            Environment.SetEnvironmentVariable(ConfigurationConstants.NuGetPackageSourceName,
                "Milou.Deployer.Web.Tests.Integration");

            Environment.SetEnvironmentVariable(
                $"{ConfigurationConstants.AutoDeployConfiguration}:default:StartupDelayInSeconds",
                "0");

            Environment.SetEnvironmentVariable(
                $"{ConfigurationConstants.AutoDeployConfiguration}:default:MetadataTimeoutInSeconds",
                "10");

            Environment.SetEnvironmentVariable(
                $"{ConfigurationConstants.AutoDeployConfiguration}:default:enabled",
                "true");

            DirectoriesToClean.Add(TestConfiguration.BaseDirectory);
        }

        protected override void OnException(Exception exception)
        {
        }

        protected override async Task BeforeStartAsync(IReadOnlyCollection<string> args)
        {
            var deploymentService = App.AppRootScope.Deepest().Lifetime.Resolve<DeploymentService>();
            var mediator = App.AppRootScope.Deepest().Lifetime.Resolve<IMediator>();

            var testTarget = new DeploymentTarget("TestTarget",
                "Test target",
                "MilouDeployerWebTest",
                allowExplicitPreRelease: false,
                autoDeployEnabled: true,
                targetDirectory: Environment.GetEnvironmentVariable("TestDeploymentTargetPath"),
                uri: Environment.GetEnvironmentVariable("TestDeploymentUri"),
                emailNotificationAddresses: new StringValues("noreply@localhost.local"));

            await mediator.Send(new CreateTarget(testTarget.Id, testTarget.Name));
            await mediator.Send(new UpdateDeploymentTarget(testTarget.Id,
                testTarget.AllowPrerelease,
                testTarget.Url,
                testTarget.PackageId,
                autoDeployEnabled: testTarget.AutoDeployEnabled,
                targetDirectory: testTarget.TargetDirectory));

            string packageVersion = "MilouDeployerWebTest 1.2.4";

            Guid deploymentTaskId = Guid.NewGuid();
            string deploymentTargetId = TestDataCreator.Testtarget;
            var deploymentTask = new DeploymentTask(packageVersion, deploymentTargetId, deploymentTaskId);

            DeploymentTaskResult deploymentTaskResult = await deploymentService.ExecuteDeploymentAsync(
                deploymentTask,
                App.Logger,
                App.CancellationTokenSource.Token);

            if (!deploymentTaskResult.ExitCode.IsSuccess)
            {
                throw new InvalidOperationException($"Initial deploy failed: {deploymentTaskResult.Metadata}");
            }

            TestStartup.TestConfiguration = TestConfiguration;

            _webHost = WebHost.CreateDefaultBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback,
                        TestSiteHttpPort.Port);
                })
                .UseContentRoot(TestConfiguration.SiteAppRoot.FullName)
                .UseStartup<TestStartup>().Build();

            await _webHost.StartAsync(App.CancellationTokenSource.Token);

            CancellationToken.Register(() => _webHost.StopAsync());
        }
    }
}