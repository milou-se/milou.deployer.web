using System;
using System.IO;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.Tests.Integration.TestData
{
    public sealed class TestConfiguration : IConfigurationValues, IDisposable
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

        public void Dispose()
        {
            if (BaseDirectory != null)
            {
                BaseDirectory.Refresh();
                BaseDirectory.Delete(true);
            }
        }
    }
}
