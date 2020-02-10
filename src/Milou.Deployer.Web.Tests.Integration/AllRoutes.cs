using System;
using System.Collections.Generic;
using System.Linq;
using Arbor.App.Extensions.Application;
using JetBrains.Annotations;
using Milou.Deployer.Web.IisHost.Areas.Settings.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class AllRoutes
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public AllRoutes(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [PublicAPI]
        public static IEnumerable<object[]> Data
        {
            get
            {
                string[] assemblyNameStartsWith = { "Milou" };
                var filteredAssemblies = ApplicationAssemblies.FilteredAssemblies(useCache: false, assemblyNameStartsWith: assemblyNameStartsWith);
                return RouteList.GetConstantRoutes(filteredAssemblies)
                    .Select(item => new object[] {item.Name, item.Value})
                    .ToArray();
            }
        }

        [MemberData(nameof(Data))]
        [Theory]
        public void ShouldStartWithSlash(string name, string value)
        {
            _testOutputHelper.WriteLine($"Asserting route {name} with value '{value}'");

            bool slashOrTildeSlash = value.StartsWith("~/", StringComparison.OrdinalIgnoreCase)
                                     || value.StartsWith("/", StringComparison.OrdinalIgnoreCase);

            Assert.True(slashOrTildeSlash);
        }
    }
}
