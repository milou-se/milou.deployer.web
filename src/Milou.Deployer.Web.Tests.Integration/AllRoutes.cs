using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Areas.Settings.Controllers;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class AllRoutes
    {
        [MemberData(nameof(Data))]
        [Theory]
        public void ShouldStartWithSlash(string name, string value)
        {
            Assert.StartsWith("/", value, StringComparison.OrdinalIgnoreCase);
        }

        [PublicAPI]
        public static IEnumerable<object[]> Data =>
            RouteList.GetConstantRoutes(AppDomain.CurrentDomain.FilteredAssemblies(useCache: false))
                .Select(item => new object[] { item.Item2, item.Item3 })
                .ToArray();
    }
}