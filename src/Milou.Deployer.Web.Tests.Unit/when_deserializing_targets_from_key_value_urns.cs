using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Machine.Specifications;
using Milou.Deployer.Web.Core.Deployment;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.Tests.Unit
{
    [Subject(typeof(DeploymentTarget))]
    public class when_deserializing_targets_from_key_value_urns
    {
        private static IKeyValueConfiguration key_value_configuration;

        private static IReadOnlyCollection<DeploymentTarget> targets;

        private Establish context = () =>
        {
            var nameValueCollection = new NameValueCollection
            {
                { "urn:milou-deployer:target", "instance1" },
                { "urn:milou-deployer:target:instance1:id", "myId1" },
                { "urn:milou-deployer:target:instance1:name", "myName1" },
                { "urn:milou-deployer:target:instance1:tool", "myTool1" },
                { "urn:milou-deployer:target:instance1:allow-Prerelease", "true" },
                {
                    "urn:milou-deployer:target:instance1:allowed-Package-Names",
                    "myAllowedPackageId1.1"
                },
                {
                    "urn:milou-deployer:target:instance1:allowed-Package-Names",
                    "myAllowedPackageId1.2"
                },
                { "urn:milou-deployer:target:instance1:uri", "http://www.google.se" },
                { "urn:milou-deployer:target:instance2:id", "myId2" },
                { "urn:milou-deployer:target:instance2:name", "myName2" },
                { "urn:milou-deployer:target:instance2:tool", "myTool2" },
                { "urn:milou-deployer:target:instance2:allow-Prerelease", "false" },
                {
                    "urn:milou-deployer:target:instance2:allowed-Package-Names",
                    "myAllowedPackageId2.1"
                },
                {
                    "urn:milou-deployer:target:instance2:allowed-Package-Names",
                    "myAllowedPackageId2.2"
                }
            };

            key_value_configuration = new InMemoryKeyValueConfiguration(nameValueCollection);
        };

        private Because of = () => { targets = key_value_configuration.GetInstances<DeploymentTarget>(); };

        private It should_not_be_null = () =>
        {
            Console.WriteLine(JsonConvert.SerializeObject(targets, Formatting.Indented));

            targets.ShouldNotBeNull();
        };
    }
}