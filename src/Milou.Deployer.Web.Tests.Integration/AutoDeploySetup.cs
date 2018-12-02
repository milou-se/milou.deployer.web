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
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.Tests.Integration.TestData;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class AutoDeploySetup : WebFixtureBase, IAppHost
    {
        private IWebHost _webHost;

        [PublicAPI]
        protected TestConfiguration TestConfiguration;

        public AutoDeploySetup(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink)
        {

            //TODO run entire test in temp dir
        }

        [PublicAPI]
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

        public override void Dispose()
        {
            base.Dispose();
        }

        public override async Task DisposeAsync()
        {
            DirectoriesToClean.Add(TestConfiguration.BaseDirectory);
            await base.DisposeAsync();
        }

        protected override async Task BeforeInitialize(CancellationToken cancellationToken)
        {
            var portPoolRange = new PortPoolRange(5200, 100);
            TestSiteHttpPort = TcpHelper.GetAvailablePort(portPoolRange);

            TestConfiguration = await new TestPathHelper().CreateTestConfigurationAsync(cancellationToken);

            Environment.SetEnvironmentVariable("TestDeploymentTargetPath", TestConfiguration.SiteAppRoot.FullName);
            Environment.SetEnvironmentVariable("TestDeploymentUri", $"http://localhost:{TestSiteHttpPort.Port}");

            string deployerDir = Path.Combine(VcsTestPathHelper.GetRootDirectory(), "tools", "milou.deployer");

            const string milouDeployerWebTestsIntegration = "Milou.Deployer.Web.Tests.Integration";

            ImmutableArray<KeyValue> keys = new List<KeyValue>
            {
                new KeyValue(Deployer.Core.Configuration.ConfigurationKeys.NuGetSource, milouDeployerWebTestsIntegration, null),
                new KeyValue(ConfigurationConstants.NugetConfigFile, TestConfiguration.NugetConfigFile.FullName, null),
                new KeyValue(Deployer.Core.Configuration.ConfigurationKeys.NuGetConfig, TestConfiguration.NugetConfigFile.FullName, null),
                new KeyValue(Deployer.Core.Configuration.ConfigurationKeys.LogLevel, "Verbose", null),
            }.ToImmutableArray();

            var jsonConfigurationSerializer = new JsonConfigurationSerializer();
            string serializedConfigurationItems = jsonConfigurationSerializer.Serialize(new ConfigurationItems("1.0", keys));

            string settingsFile = Path.Combine(deployerDir, $"{Environment.MachineName}.settings.json");

            FilesToClean.Add(new FileInfo(settingsFile));

            await File.WriteAllTextAsync(settingsFile, serializedConfigurationItems, Encoding.UTF8, cancellationToken);

            var integrationTestProjectDirectory = new DirectoryInfo(Path.Combine(VcsTestPathHelper.GetRootDirectory(),
                "src",
                milouDeployerWebTestsIntegration));
            FileInfo[] nugetPackages = integrationTestProjectDirectory.GetFiles("*.nupkg");

            if (nugetPackages.Length == 0)
            {
                throw new DeployerAppException($"Could not find nuget test packages located in {integrationTestProjectDirectory.FullName}");
            }

            foreach (FileInfo nugetPackage in nugetPackages)
            {
                nugetPackage.CopyTo(Path.Combine(TestConfiguration.NugetPackageDirectory.FullName, nugetPackage.Name));
            }

            Environment.SetEnvironmentVariable(Deployer.Core.Configuration.ConfigurationKeys.KeyValueConfigurationFile, settingsFile);

            Environment.SetEnvironmentVariable(ConfigurationConstants.NugetConfigFile,
                TestConfiguration.NugetConfigFile.FullName);

            Environment.SetEnvironmentVariable(ConfigurationConstants.NuGetPackageSourceName,
                milouDeployerWebTestsIntegration);

            Environment.SetEnvironmentVariable(
                $"{ConfigurationConstants.AutoDeployConfiguration}:default:StartupDelayInSeconds",
                "0");

            Environment.SetEnvironmentVariable(
                $"{ConfigurationConstants.AutoDeployConfiguration}:default:afterDeployDelayInSeconds",
                "1");

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
            var readService = App.AppRootScope.Deepest().Lifetime.Resolve<IDeploymentTargetReadService>();

            ImmutableArray<DeploymentTarget> targets = await readService.GetDeploymentTargetsAsync(CancellationToken);

            if (targets.Length != 1)
            {
                throw new DeployerAppException("The test target has not been created");
            }

            const string packageVersion = "MilouDeployerWebTest 1.2.4";

            Guid deploymentTaskId = Guid.NewGuid();
            const string deploymentTargetId = TestDataCreator.Testtarget;
            var deploymentTask = new DeploymentTask(packageVersion, deploymentTargetId, deploymentTaskId);

            DeploymentTaskResult deploymentTaskResult = await deploymentService.ExecuteDeploymentAsync(
                deploymentTask,
                App.Logger,
                App.CancellationTokenSource.Token);

            if (!deploymentTaskResult.ExitCode.IsSuccess)
            {
                throw new DeployerAppException($"Initial deployment failed, metadata: {deploymentTaskResult.Metadata}; test configuration: {TestConfiguration}");
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