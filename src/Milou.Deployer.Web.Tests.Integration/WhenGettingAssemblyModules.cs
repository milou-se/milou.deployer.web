﻿using System;
using System.Collections.Immutable;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class WhenGettingAssemblyModules
    {
        [Fact]
        public void ItShouldFindTestModule()
        {
            ImmutableArray<OrderedModuleRegistration> registrations = ModuleExtensions.GetModules(
                new[] { GetType().Assembly },
                Array.Empty<Type>(),
                new NoConfiguration());

            Assert.NotEmpty(registrations);
            Assert.Contains(
                registrations,
                orderedModuleRegistration =>
                    orderedModuleRegistration.ModuleRegistration.ModuleType == typeof(TestModule));
        }
    }
}