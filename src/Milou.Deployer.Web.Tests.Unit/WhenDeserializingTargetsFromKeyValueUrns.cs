using System.Collections.Specialized;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Milou.Deployer.Web.Core.Deployment;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class WhenDeserializingTargetsFromKeyValueUrns
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public WhenDeserializingTargetsFromKeyValueUrns(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Do()
        {
            var nameValueCollection = new NameValueCollection
            {
                { "urn:milou-deployer:target", "instance1" },
                { "urn:milou-deployer:target:instance1:id", "myId1" },
                { "urn:milou-deployer:target:instance1:name", "myName1" },
                { "urn:milou-deployer:target:instance1:packageId", "myAllowedPackageId1.1" },
                { "urn:milou-deployer:target:instance1:allow-Prerelease", "true" },
                {
                    "urn:milou-deployer:target:instance1:allowed-Package-Names",
                    "myAllowedPackageId1.1"
                },
                { "urn:milou-deployer:target:instance1:uri", "http://www.google.se" },
                { "urn:milou-deployer:target:instance2:id", "myId2" },
                { "urn:milou-deployer:target:instance2:name", "myName2" },
                { "urn:milou-deployer:target:instance2:packageId", "myAllowedPackageId2.1" },
                { "urn:milou-deployer:target:instance2:allow-Prerelease", "false" }
            };

            var keyValueConfiguration = new InMemoryKeyValueConfiguration(nameValueCollection);

            var targets = keyValueConfiguration.GetInstances<DeploymentTarget>();

            _testOutputHelper.WriteLine(JsonConvert.SerializeObject(targets, Formatting.Indented));

            Assert.NotEqual(default, targets);
        }
    }
}
