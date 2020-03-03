using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arbor.App.Extensions;
using MediatR;
using Milou.Deployer.Web.Agent;
using Milou.Deployer.Web.Agent.Host;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Controllers;
using Milou.Deployer.Web.Marten;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class CodeQualityTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public CodeQualityTests(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

        [MemberData(nameof(GetAssemblyTypes))]
        [Theory]
        public void ShouldNotContainImplementations(Assembly assembly, Type checkForType)
        {
            var currentTypes = assembly.GetLoadableTypes();
            var types = currentTypes.ToDictionary(type => type, _ => true);

            foreach (var currentType in currentTypes)
            {
                bool currentTypeIsClass = !currentType.IsAbstract && currentType.IsClass &&
                                          checkForType.IsAssignableFrom(currentType);

                if (currentTypeIsClass)
                {
                    _testOutputHelper.WriteLine($"Assembly {assembly.GetName().Name} and type {currentType.FullName} is implementing {checkForType.FullName}");
                }

                types[currentType] = currentTypeIsClass;
            }

            var errors = types.Where(pair => pair.Value).Select(pair => pair.Key).SafeToImmutableArray();

            Assert.Empty(errors);
        }

        [MemberData(nameof(GetMartenDataTypes))]
        [Theory]
        public void ShouldNotContainMartenData(Assembly assembly, Type checkForType)
        {
            var types = assembly.GetLoadableTypes();

            foreach (var currentType in types)
            {
                Assert.Null(currentType.GetCustomAttribute(checkForType));
            }
        }

        public static IEnumerable<object[]> GetAssemblyTypes()
        {
            var notificationType = typeof(INotification);

            yield return new object[] {typeof(BaseApiController).Assembly, notificationType};
            yield return new object[] {typeof(IDeploymentPackageAgent).Assembly, notificationType};
            yield return new object[] {typeof(AgentService).Assembly, notificationType};
        }

        public static IEnumerable<object[]> GetMartenDataTypes()
        {
            yield return new object[] {typeof(BaseApiController).Assembly, typeof(MartenDataAttribute)};
            yield return new object[] {typeof(DeploymentTarget).Assembly, typeof(MartenDataAttribute)};
            yield return new object[] {typeof(IDeploymentPackageAgent).Assembly, typeof(MartenDataAttribute)};
            yield return new object[] {typeof(AgentService).Assembly, typeof(MartenDataAttribute)};
        }
    }
}