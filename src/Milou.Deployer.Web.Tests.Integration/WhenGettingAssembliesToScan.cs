using System;
using System.Collections.Immutable;
using System.Linq;
using Arbor.App.Extensions.Application;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers;
using Milou.Deployer.Web.Marten;
using Xunit;
using Xunit.Abstractions;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class WhenGettingAssembliesToScan
    {
        public WhenGettingAssembliesToScan(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        [Fact]
        public void ItShouldFindAllKnownAssemblies()
        {
            var assemblies = ApplicationAssemblies.FilteredAssemblies(useCache: false)
                .Where(assembly => !assembly.GetName().Name.EndsWith(".Views", StringComparison.OrdinalIgnoreCase))
                .ToImmutableArray();

            _output.WriteLine(string.Join(
                Environment.NewLine,
                assemblies.Select(assembly => $"{assembly.FullName} {assembly.Location}")));

            Assert.Equal(4, assemblies.Length);

            Assert.Contains(assemblies, assembly => assembly == typeof(DeployController).Assembly);
            Assert.Contains(assemblies, assembly => assembly == typeof(VcsTestPathHelper).Assembly);
            Assert.Contains(assemblies, assembly => assembly == typeof(ApplicationAssemblies).Assembly);
            Assert.Contains(assemblies, assembly => assembly == typeof(MartenConfiguration).Assembly);
        }
    }
}
