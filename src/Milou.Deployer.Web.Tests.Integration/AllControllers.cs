using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class AllControllers
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public AllControllers(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [MemberData(nameof(Data))]
        [Theory]
        public void ShouldHaveAnonymousOrAuthorize(string controller, string assembly)
        {
            Type controllerType = Type.GetType($"{controller}, {assembly}");

            Type[] httpMethodAttributes = {
                typeof(AuthorizeAttribute),
                typeof(AllowAnonymousAttribute),
            };

            Attribute[] attributes = controllerType.GetCustomAttributes().Where(attribute =>
                    httpMethodAttributes.Any(authenticationAttribute => authenticationAttribute == attribute.GetType()))
                .ToArray();

            _testOutputHelper.WriteLine($"Controller '{controller}' attributes: {attributes.Length}");

            Assert.Single(attributes);
        }

        [PublicAPI]
        public static IEnumerable<object[]> Data =>
            Assemblies.FilteredAssemblies(useCache: false).SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(type => !type.IsAbstract && typeof(Controller).IsAssignableFrom(type))
                .Select(item => new object[] { item.FullName, item.Assembly.GetName().Name })
                .ToArray();
    }
}