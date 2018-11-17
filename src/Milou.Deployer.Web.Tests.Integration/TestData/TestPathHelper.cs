using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Tests.Integration.TestData
{
    public class TestPathHelper
    {
        public async Task<TestConfiguration> CreateTestConfigurationAsync(CancellationToken cancellationToken)
        {
            string projectName = "Milou.Deployer.Web.Tests.Integration";

            string baseDirectoryPath = Path.Combine(Path.GetTempPath(),
                projectName + "-" + DateTime.UtcNow.Ticks.ToString());

            var baseDirectory = new DirectoryInfo(baseDirectoryPath);

            baseDirectory.Create();
            DirectoryInfo targetAppRoot = baseDirectory.CreateSubdirectory("target");
            DirectoryInfo nugetBaseDirectory = baseDirectory.CreateSubdirectory("nuget");
            DirectoryInfo nugetPackageDirectory = nugetBaseDirectory.CreateSubdirectory("packages");

            var nugetConfigFile =
                new FileInfo(Path.Combine(nugetBaseDirectory.FullName, $"{projectName}.nuget.config"));

            await NuGetConfigCreator.CreateNuGetConfig(nugetConfigFile, nugetPackageDirectory, cancellationToken);

            var testConfiguration = new TestConfiguration(baseDirectory,
                nugetConfigFile,
                nugetPackageDirectory,
                targetAppRoot);

            string nugetConfigContent = await File.ReadAllTextAsync(nugetConfigFile.FullName, cancellationToken);

            Console.WriteLine(
                $"Created test configuration {testConfiguration} with nuget config file content {Environment.NewLine}{nugetConfigContent}");

            return testConfiguration;
        }
    }
}