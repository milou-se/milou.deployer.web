using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Application;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class AllControllerActions
    {
        public AllControllerActions(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;

        [MemberData(nameof(Data))]
        [Theory]
        public void ShouldHaveExplicitHttpMethodAttribute(string controller, string assembly, string action)
        {
            var type = Type.GetType($"{controller}, {assembly}");

            var actionMethod = type.GetMethods()
                .Where(method => method.Name.Equals(action, StringComparison.OrdinalIgnoreCase)).ToArray();

            Type[] httpMethodAttributes =
            {
                typeof(HttpPostAttribute), typeof(HttpGetAttribute), typeof(HttpDeleteAttribute)
            };

            foreach (var methodInfo in actionMethod)
            {
                var attributes = methodInfo.GetCustomAttributes()
                    .Where(attribute =>
                        httpMethodAttributes.Any(httpMethodAttribute => httpMethodAttribute == attribute.GetType()))
                    .ToArray();

                _testOutputHelper.WriteLine(
                    $"Controller '{controller}' with action '{action}' has http method attribute: {attributes.Length == 1}");

                Assert.NotEmpty(attributes);
                Assert.Single(attributes);
            }
        }

        [PublicAPI]
        public static IEnumerable<object[]> Data =>
            ApplicationAssemblies.FilteredAssemblies(useCache: false)
                .Concat(new[] { typeof(DeployController).Assembly })
                .Distinct()
                .SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(type => !type.IsAbstract && typeof(Controller).IsAssignableFrom(type))
                .Select(controllerType => (Controller: controllerType,
                    Actions: controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance |
                                                       BindingFlags.DeclaredOnly)))
                .SelectMany(item => item.Actions.Select(action => (item.Controller, Action: action)))
                .Select(item => new object[]
                {
                    item.Controller.FullName, item.Controller.Assembly.GetName().Name, item.Action.Name
                })
                .ToArray();

        [Fact]
        public void ShouldFindControllers()
        {
            Assert.NotEmpty(Data);
        }
    }
}
