using Milou.Deployer.Web.Core.Deployment;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class EnvironmentConfigTests
    {
        [Fact]
        public void GetConfigFromTargetWithEmptyOldEnvironment()
        {
            DeploymentTarget target = new DeploymentTarget("123", "123", "abc",
                environmentConfiguration: "");

            string? environmentConfig = target.GetEnvironmentConfiguration();

            Assert.Null(environmentConfig);
        }

        [Fact]
        public void GetConfigFromTargetWithOldEnvironment()
        {
            DeploymentTarget target = new DeploymentTarget("123", "123", "abc",
                environmentConfiguration: "test");

            string? environmentConfig = target.GetEnvironmentConfiguration();

            Assert.Equal("test", environmentConfig);
        }

        [Fact]
        public void GetConfigFromTargetWithOtherEnvironment()
        {
            DeploymentTarget target = new DeploymentTarget("123", "123", "abc",
                environmentType: new EnvironmentType("", "", PreReleaseBehavior.Allow));

            string? environmentConfig = target.GetEnvironmentConfiguration();

            Assert.Null(environmentConfig);
        }

        [Fact]
        public void GetConfigFromTargetWithoutEnvironment()
        {
            DeploymentTarget target = new DeploymentTarget("123", "123", "abc");

            string? environmentConfig = target.GetEnvironmentConfiguration();

            Assert.Null(environmentConfig);
        }

        [Fact]
        public void GetConfigFromTargetWithUnknownEnvironment()
        {
            DeploymentTarget target = new DeploymentTarget("123", "123", "abc",
                environmentType: EnvironmentType.Unknown);

            string? environmentConfig = target.GetEnvironmentConfiguration();

            Assert.Null(environmentConfig);
        }
    }
}