using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Targets;

namespace Milou.Deployer.Web.Tests.Integration
{
    public static class TestDataCreator
    {
        public const string Testtarget = "TestTarget";

        public static Task<IReadOnlyCollection<OrganizationInfo>> CreateData()
        {
            var targets = new List<OrganizationInfo>
            {
                new OrganizationInfo("testorg",
                    new List<ProjectInfo>
                    {
                        new ProjectInfo("testorg",
                            "testproject",
                            new List<DeploymentTarget>
                            {
                                new DeploymentTarget(Testtarget,
                                    "Test target",
                                    "MilouDeployerWebTest",
                                    allowExplicitPreRelease: false,
                                    autoDeployEnabled: true,
                                    targetDirectory: Environment.GetEnvironmentVariable("TestDeploymentTargetPath"),
                                    uri: Environment.GetEnvironmentVariable("TestDeploymentUri"),
                                    emailNotificationAddresses: new StringValues("noreply@localhost.local"),
                                    enabled: true)
                            })
                    })
            };

            return Task.FromResult<IReadOnlyCollection<OrganizationInfo>>(targets);
        }
    }
}
