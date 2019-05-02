using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Schema.Json;
using JetBrains.Annotations;
using Milou.Deployer.Core.Configuration;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Tests.Integration.TestData;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    [UsedImplicitly]
    public class AutoDeploySetup : WebFixtureBase, IAppHost
    {
        public AutoDeploySetup(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink)
        {
            // TODO run entire test in temp dir
        }

        public override async Task DisposeAsync()
        {
            if (TestConfiguration?.BaseDirectory != null)
            {
                DirectoriesToClean.Add(TestConfiguration.BaseDirectory);
            }

            await base.DisposeAsync();
        }

        protected override Task RunAsync()
        {
            return Task.CompletedTask;
        }

        protected override async Task BeforeInitialize(CancellationToken cancellationToken)
        {
            TestConfiguration = await TestPathHelper.CreateTestConfigurationAsync(CancellationToken.None);

            var portPoolRange = new PortPoolRange(5200, 100);
            TestSiteHttpPort = new TestHttpPort(TcpHelper.GetAvailablePort(portPoolRange));

            Environment.SetEnvironmentVariable("TestDeploymentTargetPath", TestConfiguration.SiteAppRoot.FullName);
            Environment.SetEnvironmentVariable("TestDeploymentUri", $"http://localhost:{TestSiteHttpPort.Port.Port+1}");

            var deployerDir = Path.Combine(VcsTestPathHelper.GetRootDirectory(), "tools", "milou.deployer");

            const string milouDeployerWebTestsIntegration = "Milou.Deployer.Web.Tests.Integration";

            var keys = new List<KeyValue>
            {
                new KeyValue(ConfigurationKeys.NuGetSource, milouDeployerWebTestsIntegration, null),
                new KeyValue(ConfigurationConstants.NugetConfigFile,
                    TestConfiguration.NugetConfigFile.FullName,
                    null),
                new KeyValue(ConfigurationKeys.NuGetConfig, TestConfiguration.NugetConfigFile.FullName, null),
                new KeyValue(ConfigurationKeys.LogLevel, "Verbose", null)
            }.ToImmutableArray();

            var serializedConfigurationItems =
                JsonConfigurationSerializer.Serialize(new ConfigurationItems("1.0", keys));

            var settingsFile = Path.Combine(deployerDir, $"{Environment.MachineName}.settings.json");

            FilesToClean.Add(new FileInfo(settingsFile));

            await File.WriteAllTextAsync(settingsFile, serializedConfigurationItems, Encoding.UTF8, cancellationToken);

            var integrationTestProjectDirectory = new DirectoryInfo(Path.Combine(VcsTestPathHelper.GetRootDirectory(),
                "src",
                milouDeployerWebTestsIntegration));

            var nugetPackages = integrationTestProjectDirectory.GetFiles("*.nupkg");

            if (nugetPackages.Length == 0)
            {
                throw new DeployerAppException(
                    $"Could not find nuget test packages located in {integrationTestProjectDirectory.FullName}");
            }

            foreach (var nugetPackage in nugetPackages)
            {
                nugetPackage.CopyTo(Path.Combine(TestConfiguration.NugetPackageDirectory.FullName, nugetPackage.Name));
            }

            Environment.SetEnvironmentVariable(ConfigurationKeys.KeyValueConfigurationFile, settingsFile);

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

        protected override Task BeforeStartAsync(IReadOnlyCollection<string> args)
        {
            return Task.CompletedTask;
        }
    }
}
