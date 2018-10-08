using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class AllControllerActions
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public AllControllerActions(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [MemberData(nameof(Data))]
        [Theory]
        public void ShouldHaveExplicitHttpMethodAttribute(string controller, string assembly, string action)
        {
            Type type = Type.GetType($"{controller}, {assembly}");

            MethodInfo[] actionMethod = type.GetMethods().Where(method => method.Name.Equals(action, StringComparison.OrdinalIgnoreCase)).ToArray();

            Type[] httpMethodAttributes = {
                typeof(HttpPostAttribute),
                typeof(HttpGetAttribute),
                typeof(HttpDeleteAttribute),
            };

            foreach (MethodInfo methodInfo in actionMethod)
            {
                Attribute[] attributes = methodInfo.GetCustomAttributes()
                    .Where(attribute =>
                        httpMethodAttributes.Any(httpMethodAttribute => httpMethodAttribute == attribute.GetType()))
                    .ToArray();

                _testOutputHelper.WriteLine($"Controller '{controller}' with action '{action}' is has http method attribute: {attributes.Length == 1}");

                Assert.Single(attributes);
            }
        }

        [PublicAPI]
        public static IEnumerable<object[]> Data =>
            Assemblies.FilteredAssemblies(useCache: false).SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(type => !type.IsAbstract && typeof(Controller).IsAssignableFrom(type))
                .Select(controllerType => (Controller:controllerType, Actions:controllerType.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly)))
                .SelectMany(item => item.Actions.Select(action => (item.Controller, Action:action)))
                .Select(item =>new object[] { item.Controller.FullName, item.Controller.Assembly.GetName().Name, item.Action.Name })
                .ToArray();
    }
}