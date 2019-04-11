using System;
using System.Collections.Specialized;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class WhenGettingAssemblyModulesForTypesWithIKeyValueConfigurationDependency
    {
        [Fact]
        public void ItShouldInstantiateModule()
        {
            var nameValueCollection = new NameValueCollection
            {
                { nameof(NoOpTestModule.MeaningOfLife), "42" }
            };

            var registrations = ModuleExtensions.GetModules(
                new[] { GetType().Assembly },
                Array.Empty<Type>(),
                new InMemoryKeyValueConfiguration(nameValueCollection));

            var orderedModuleRegistration =
                registrations.SingleOrDefault(moduleRegistration =>
                    moduleRegistration.ModuleRegistration.ModuleType == typeof(NoOpTestModule));

            Assert.NotNull(orderedModuleRegistration);

            var module = orderedModuleRegistration.Module as NoOpTestModule;

            Assert.NotNull(module);

            Assert.Equal("42", module.MeaningOfLife);
        }
    }
}
