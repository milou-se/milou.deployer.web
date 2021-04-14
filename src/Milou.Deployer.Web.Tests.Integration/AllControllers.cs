using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.ExtensionMethods;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class AllControllers
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public AllControllers(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

        [PublicAPI]
        public static IEnumerable<object[]> Data
        {
            get
            {
                string[] assemblyNameStartsWith = new[] {"Milou"};
                var filteredAssemblies = ApplicationAssemblies.FilteredAssemblies(useCache: false, assemblyNameStartsWith: assemblyNameStartsWith);

                return filteredAssemblies
                    .SelectMany(assembly => assembly.GetLoadableTypes())
                    .Where(type => !type.IsAbstract && typeof(Controller).IsAssignableFrom(type))
                    .Select(type => new object[] {type.AssemblyQualifiedName})
                    .ToArray();
            }
        }

        [MemberData(nameof(Data))]
        [Theory]
        public void ShouldHaveAnonymousOrAuthorize(string qualifiedName)
        {
            var controllerType = Type.GetType(qualifiedName);

            Type[] httpMethodAttributes =
            {
                typeof(AuthorizeAttribute),
                typeof(AllowAnonymousAttribute)
            };

            var attributes = controllerType.GetCustomAttributes().Where(attribute =>
                    httpMethodAttributes.Any(authenticationAttribute => authenticationAttribute == attribute.GetType()))
                .ToArray();

            _testOutputHelper.WriteLine(
                $"Controller '{controllerType.Name}' anonymous or authorization attributes: {attributes.Length}, expected is 1");

            Assert.NotEmpty(attributes);
            Assert.Single(attributes);
        }
    }
}
