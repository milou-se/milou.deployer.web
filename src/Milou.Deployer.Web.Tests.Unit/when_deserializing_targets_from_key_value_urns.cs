using System;
using System.Collections.Specialized;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Milou.Deployer.Web.Core.Deployment;
using Newtonsoft.Json;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class when_deserializing_targets_from_key_value_urns
    {
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

            var key_value_configuration = new InMemoryKeyValueConfiguration(nameValueCollection);

            var targets = key_value_configuration.GetInstances<DeploymentTarget>();

            Console.WriteLine(JsonConvert.SerializeObject(targets, Formatting.Indented));

            Assert.NotEqual(default, targets);
        }
    }
}