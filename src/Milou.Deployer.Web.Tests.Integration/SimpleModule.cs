using System;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class SimpleModule
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
                    moduleRegistration.ModuleRegistration.ModuleType == typeof(SimpleNoOpTestModule));

            Assert.NotNull(orderedModuleRegistration);
        }

        [Fact]
        public void ShouldHaveNoTag()
        {
            var registrations = ModuleExtensions.GetModules(
                new[] { GetType().Assembly },
                Array.Empty<Type>(),
                new NoConfiguration());

            var orderedModuleRegistration =
                registrations.SingleOrDefault(moduleRegistration =>
                    moduleRegistration.ModuleRegistration.ModuleType == typeof(SimpleNoOpTestModule));

            Assert.Null(orderedModuleRegistration?.ModuleRegistration?.Tag);
        }

        [Fact]
        public void ShouldHaveOrder0()
        {
            var registrations = ModuleExtensions.GetModules(
                new[] { GetType().Assembly },
                Array.Empty<Type>(),
                new NoConfiguration());

            var orderedModuleRegistration =
                registrations.SingleOrDefault(moduleRegistration =>
                    moduleRegistration.ModuleRegistration.ModuleType == typeof(SimpleNoOpTestModule));

            Assert.Equal(0, orderedModuleRegistration?.ModuleRegistration?.Order);
        }

        [Fact]
        public void ShouldRegisterInRootScopeFalse()
        {
            var registrations = ModuleExtensions.GetModules(
                new[] { GetType().Assembly },
                Array.Empty<Type>(),
                new NoConfiguration());

            var orderedModuleRegistration =
                registrations.SingleOrDefault(moduleRegistration =>
                    moduleRegistration.ModuleRegistration.ModuleType == typeof(SimpleNoOpTestModule));

            Assert.False(orderedModuleRegistration?.ModuleRegistration?.RegisterInRootScope);
        }
    }
}
