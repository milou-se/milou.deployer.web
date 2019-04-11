using System;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class ModuleWithOrderAndTag
    {
        [Fact]
        public void ShouldBeFound()
        {
            var registrations = ModuleExtensions.GetModules(
                new[] { GetType().Assembly },
                Array.Empty<Type>(),
                new NoConfiguration());

            var orderedModuleRegistration =
                registrations.SingleOrDefault(moduleRegistration =>
                    moduleRegistration.ModuleRegistration.ModuleType == typeof(NoOpTestModule));

            Assert.Equal(37, orderedModuleRegistration?.ModuleRegistration?.Order);
            Assert.Equal(Scope.AspNetCoreScope, orderedModuleRegistration?.ModuleRegistration?.Tag);
            Assert.True(orderedModuleRegistration?.ModuleRegistration?.RegisterInRootScope);
        }
    }
}
