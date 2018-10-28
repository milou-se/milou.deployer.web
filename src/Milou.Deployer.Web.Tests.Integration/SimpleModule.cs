using System;
using System.Collections.Immutable;
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
            ImmutableArray<OrderedModuleRegistration> registrations = ModuleExtensions.GetModules(
                new[] { GetType().Assembly },
                Array.Empty<Type>(),
                new NoConfiguration());

            OrderedModuleRegistration orderedModuleRegistration =
                registrations.SingleOrDefault(moduleRegistration =>
                    moduleRegistration.ModuleRegistration.ModuleType == typeof(SimpleNoOpTestModule));

            Assert.NotNull(orderedModuleRegistration);
        }

        [Fact]
        public void ShouldHaveNoTag()
        {
            ImmutableArray<OrderedModuleRegistration> registrations = ModuleExtensions.GetModules(
                new[] { GetType().Assembly },
                Array.Empty<Type>(),
                new NoConfiguration());

            OrderedModuleRegistration orderedModuleRegistration =
                registrations.SingleOrDefault(moduleRegistration =>
                    moduleRegistration.ModuleRegistration.ModuleType == typeof(SimpleNoOpTestModule));

            Assert.Null(orderedModuleRegistration?.ModuleRegistration?.Tag);
        }

        [Fact]
        public void ShouldHaveOrder0()
        {
            ImmutableArray<OrderedModuleRegistration> registrations = ModuleExtensions.GetModules(
                new[] { GetType().Assembly },
                Array.Empty<Type>(),
                new NoConfiguration());

            OrderedModuleRegistration orderedModuleRegistration =
                registrations.SingleOrDefault(moduleRegistration =>
                    moduleRegistration.ModuleRegistration.ModuleType == typeof(SimpleNoOpTestModule));

            Assert.Equal(0, orderedModuleRegistration?.ModuleRegistration?.Order);
        }

        [Fact]
        public void ShouldRegisterInRootScopeFalse()
        {
            ImmutableArray<OrderedModuleRegistration> registrations = ModuleExtensions.GetModules(
                new[] { GetType().Assembly },
                Array.Empty<Type>(),
                new NoConfiguration());

            OrderedModuleRegistration orderedModuleRegistration =
                registrations.SingleOrDefault(moduleRegistration =>
                    moduleRegistration.ModuleRegistration.ModuleType == typeof(SimpleNoOpTestModule));

            Assert.False(orderedModuleRegistration?.ModuleRegistration?.RegisterInRootScope);
        }
    }
}