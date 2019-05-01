using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Tests.Integration.TestData
{
    public static class TestPathHelper
    {
        public static async Task<TestConfiguration> CreateTestConfigurationAsync(CancellationToken cancellationToken)
        {
            var projectName = "Milou.Deployer.Web.Tests.Integration";

            var baseDirectoryPath = Path.Combine(Path.GetTempPath(),
                projectName + "-" + DateTime.UtcNow.Ticks);

            var baseDirectory = new DirectoryInfo(baseDirectoryPath);

            baseDirectory.Create();
            var targetAppRoot = baseDirectory.CreateSubdirectory("target");
            var nugetBaseDirectory = baseDirectory.CreateSubdirectory("nuget");
            var nugetPackageDirectory = nugetBaseDirectory.CreateSubdirectory("packages");

            var nugetConfigFile =
                new FileInfo(Path.Combine(nugetBaseDirectory.FullName, $"{projectName}.nuget.config"));

            await NuGetConfigCreator.CreateNuGetConfig(nugetConfigFile, nugetPackageDirectory, cancellationToken);

            var testConfiguration = new TestConfiguration(baseDirectory,
                nugetConfigFile,
                nugetPackageDirectory,
                targetAppRoot);

            var nugetConfigContent = await File.ReadAllTextAsync(nugetConfigFile.FullName, cancellationToken);

            Console.WriteLine(
                $"Created test configuration {testConfiguration} with nuget config file content {Environment.NewLine}{nugetConfigContent}");

            return testConfiguration;
        }
    }
}
