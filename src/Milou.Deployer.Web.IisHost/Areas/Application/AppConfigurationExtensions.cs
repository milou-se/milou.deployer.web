using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Autofac;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class AppConfigurationExtensions
    {
        public static ImmutableArray<(object, string)> GetConfigurationValues(this Scope scope)
        {
            var configurationValueTypes = Assemblies.FilteredAssemblies()
                .SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(t => t.IsPublicConcreteTypeImplementing<IConfigurationValues>())
                .ToImmutableArray();

            var logItems = new List<(object, string)>();

            foreach (var configurationValueType in configurationValueTypes)
            {
                if (ResolutionExtensions.TryResolve(scope.Lifetime, configurationValueType, out var instance))
                {
                    var toString = instance.ToString();

                    var typeFullName = configurationValueType.FullName;

                    if (toString.Equals(typeFullName, StringComparison.OrdinalIgnoreCase))
                    {
                        var asJson = JsonConvert.SerializeObject(instance);

                        logItems.Add((instance, asJson));
                    }
                    else
                    {
                        logItems.Add((instance, toString));
                    }
                }
            }

            return logItems.ToImmutableArray();
        }
    }
}
