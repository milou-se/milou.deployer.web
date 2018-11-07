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
            AppVersionInfo appVersionInfo = VersionHelper.GetAppVersion();

            _output.WriteLine(appVersionInfo.AssemblyVersion);
            _output.WriteLine(appVersionInfo.FileVersion);
            _output.WriteLine(appVersionInfo.AssemblyFullName);
            _output.WriteLine(appVersionInfo.InformationalVersion);

            Assert.NotNull(appVersionInfo.AssemblyVersion);
            Assert.NotNull(appVersionInfo.FileVersion);
            Assert.NotNull(appVersionInfo.AssemblyFullName);
            Assert.NotNull(appVersionInfo.InformationalVersion);
        }
    }
}