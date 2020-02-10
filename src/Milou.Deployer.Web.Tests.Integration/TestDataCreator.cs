using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Microsoft.Extensions.Primitives;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Targets;

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
                                    url: Environment.GetEnvironmentVariable("TestDeploymentUri").ParseUriOrDefault(),
                                    emailNotificationAddresses: new StringValues("noreply@localhost.local"),
                                    enabled: true)
                            })
                    })
            };

            return Task.FromResult<IReadOnlyCollection<OrganizationInfo>>(targets);
        }
    }
}
