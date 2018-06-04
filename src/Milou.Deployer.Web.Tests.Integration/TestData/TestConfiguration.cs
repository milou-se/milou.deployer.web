using System.IO;

namespace Milou.Deployer.Web.Tests.Integration.TestData
{
    public class TestConfiguration
    {
        public TestConfiguration(
            DirectoryInfo baseDirectory,
            FileInfo nugetConfigFile,
            DirectoryInfo nugetPackageDirectory,
            DirectoryInfo siteAppRoot)
        {
            BaseDirectory = baseDirectory;
            NugetConfigFile = nugetConfigFile;
            NugetPackageDirectory = nugetPackageDirectory;
            SiteAppRoot = siteAppRoot;
        }

        public DirectoryInfo BaseDirectory { get; }

        public FileInfo NugetConfigFile { get; }

        public DirectoryInfo NugetPackageDirectory { get; }

        public DirectoryInfo SiteAppRoot { get; }
    }
}