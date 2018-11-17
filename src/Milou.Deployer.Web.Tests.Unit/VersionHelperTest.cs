using Milou.Deployer.Web.Core;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class VersionHelperTest
    {
        private readonly ITestOutputHelper _output;

        public VersionHelperTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void VersionInfoShouldNotBeNull()
        {
            ApplicationVersionInfo applicationVersionInfo = ApplicationVersionHelper.GetAppVersion();

            _output.WriteLine(applicationVersionInfo.AssemblyVersion);
            _output.WriteLine(applicationVersionInfo.FileVersion);
            _output.WriteLine(applicationVersionInfo.AssemblyFullName);
            _output.WriteLine(applicationVersionInfo.InformationalVersion);

            Assert.NotNull(applicationVersionInfo.AssemblyVersion);
            Assert.NotNull(applicationVersionInfo.FileVersion);
            Assert.NotNull(applicationVersionInfo.AssemblyFullName);
            Assert.NotNull(applicationVersionInfo.InformationalVersion);
        }
    }
}