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

        [PublicAPI]
        public static IEnumerable<object[]> Data =>
            Assemblies.FilteredAssemblies(useCache: false)
                .SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(type => !type.IsAbstract && typeof(Controller).IsAssignableFrom(type))
                .Select(type => new object[] { type.FullName, type.Assembly.GetName().Name })
                .ToArray();

        [MemberData(nameof(Data))]
        [Theory]
        public void ShouldHaveAnonymousOrAuthorize(string controller, string assembly)
        {
            var controllerType = Type.GetType($"{controller}, {assembly}");

            Type[] httpMethodAttributes =
            {
                typeof(AuthorizeAttribute),
                typeof(AllowAnonymousAttribute)
            };

            var attributes = controllerType.GetCustomAttributes().Where(attribute =>
                    httpMethodAttributes.Any(authenticationAttribute => authenticationAttribute == attribute.GetType()))
                .ToArray();

            _testOutputHelper.WriteLine(
                $"Controller '{controller}' anonymous or authorization attributes: {attributes.Length}, expected is 1");

            Assert.NotEmpty(attributes);
            Assert.Single(attributes);
        }
    }
}
