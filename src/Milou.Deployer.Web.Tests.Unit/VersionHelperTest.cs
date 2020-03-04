﻿using Arbor.App.Extensions.Application;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class VersionHelperTest
    {
        public VersionHelperTest(ITestOutputHelper output) => _output = output;

        private readonly ITestOutputHelper _output;

        [Fact]
        public void VersionInfoShouldNotBeNull()
        {
            var applicationVersionInfo = ApplicationVersionHelper.GetAppVersion();

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