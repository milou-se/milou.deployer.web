using System.IO;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.Tests.Integration.TestData
{
    public class TestConfiguration : IConfigurationValues
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

        public override string ToString()
        {
            return
                $"{nameof(BaseDirectory)}: {BaseDirectory.FullName}, {nameof(NugetConfigFile)}: {NugetConfigFile.FullName}, {nameof(NugetPackageDirectory)}: {NugetPackageDirectory.FullName}, {nameof(SiteAppRoot)}: {SiteAppRoot.FullName}";
        }
    }
}