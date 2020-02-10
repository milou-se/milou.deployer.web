using System;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Json;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class WhenSerializingADeploymentTarget
    {
        public WhenSerializingADeploymentTarget(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;

        [Fact]
        public void ItShouldBeDeserializable()
        {
            var target = new DeploymentTarget("myid", "myName", "tool");

            var json = JsonConvert.SerializeObject(target,
                Formatting.Indented,
                new JsonSerializerSettings().UseCustomConverters());

            _testOutputHelper.WriteLine(json);

            var deserialized = JsonConvert.DeserializeObject<DeploymentTarget>(json);

            _testOutputHelper.WriteLine(deserialized.ToString());

            Assert.NotNull(deserialized);

            Assert.Null(deserialized.NuGet);
            Assert.Null(deserialized.NuGet?.NuGetPackageSource);
            Assert.Null(deserialized.NuGet?.NuGetConfigFile);
        }

        [Fact]
        public void ItShouldBeDeserializableWithNuget()
        {
            var target = new DeploymentTarget("myid", "myName", "tool",
                nuget: new TargetNuGetSettings
                {
                    NuGetConfigFile = "123",
                    NuGetPackageSource = "source-abc",
                    PackageListTimeout = TimeSpan.FromMinutes(1)
                });

            var json = JsonConvert.SerializeObject(target,
                Formatting.Indented,
                new JsonSerializerSettings().UseCustomConverters());

            _testOutputHelper.WriteLine(json);

            var deserialized = JsonConvert.DeserializeObject<DeploymentTarget>(json);

            _testOutputHelper.WriteLine(deserialized.ToString());

            Assert.NotNull(deserialized);

            Assert.NotNull(deserialized.NuGet);
            Assert.NotNull(deserialized.NuGet?.NuGetPackageSource);
            Assert.NotNull(deserialized.NuGet?.NuGetConfigFile);
        }
    }
}
