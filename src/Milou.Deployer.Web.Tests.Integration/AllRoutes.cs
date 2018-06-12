using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Milou.Deployer.Web.IisHost.Areas.Application;
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
        public static IEnumerable<object[]> Data =>
            RouteList.GetConstantRoutes(AppDomain.CurrentDomain.FilteredAssemblies(useCache: false))
                .Select(item => new object[] { item.Item2, item.Item3 })
                .ToArray();

        [MemberData(nameof(Data))]
        [Theory]
        public void ShouldStartWithSlash(string name, string value)
        {
            _testOutputHelper.WriteLine($"Asserting route {name} with value '{value}'");
            Assert.StartsWith("/", value, StringComparison.OrdinalIgnoreCase);
        }
    }
}