using System.Collections.Generic;
using Arbor.AspNetCore.Host.Configuration;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class ConfigurationInitializationTests
    {
        [Fact]
        public void WhenInitializedWithCommandLineArg()
        {
            var args = new[] { "urn:abc:123=hello world" };
            var multiSourceKeyValueConfiguration =
                ConfigurationInitialization.InitializeConfiguration(args: args);

            Assert.NotNull(multiSourceKeyValueConfiguration);
            Assert.Equal("hello world", multiSourceKeyValueConfiguration["urn:abc:123"]);
        }

        [Fact]
        public void WhenInitializedWithEnvironmentVariable()
        {
            var args = new Dictionary<string, string> { ["urn:abc:123"] = "hello world" };
            var multiSourceKeyValueConfiguration =
                ConfigurationInitialization.InitializeConfiguration(environmentVariables: args);

            Assert.NotNull(multiSourceKeyValueConfiguration);
            Assert.Equal("hello world", multiSourceKeyValueConfiguration["urn:abc:123"]);
        }

        [Fact]
        public void WhenInitializedWithEnvironmentVariableAndCommandLineArgs()
        {
            var args = new[] { "urn:abc:123=hello arg world" };
            var environmentVariables = new Dictionary<string, string> { ["urn:abc:123"] = "hello environment world" };
            var multiSourceKeyValueConfiguration =
                ConfigurationInitialization.InitializeConfiguration(environmentVariables: environmentVariables,
                    args: args);

            Assert.NotNull(multiSourceKeyValueConfiguration);
            Assert.Equal("hello arg world", multiSourceKeyValueConfiguration["urn:abc:123"]);
        }

        [Fact]
        public void WhenInitializedWithNoParameters()
        {
            var multiSourceKeyValueConfiguration =
                ConfigurationInitialization.InitializeConfiguration();

            Assert.NotNull(multiSourceKeyValueConfiguration);
        }
    }
}
