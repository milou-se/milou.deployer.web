using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Newtonsoft.Json;
using ResolutionExtensions = Autofac.ResolutionExtensions;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class AppConfigurationExtensions
    {
        public static ImmutableArray<(object, string)> GetConfigurationValues(this App app)
        {
            ImmutableArray<Type> configurationValueTypes = Assemblies.FilteredAssemblies().SelectMany(assembly => TypeExtensions.GetLoadableTypes(assembly))
                .Where(t => t.IsPublicConcreteTypeImplementing<IConfigurationValues>())
                .ToImmutableArray();

            var logItems = new List<(object, string)>();

            foreach (Type configurationValueType in configurationValueTypes)
            {
                if (ResolutionExtensions.TryResolve(app.AppRootScope.Deepest().Lifetime, configurationValueType, out object instance))
                {
                    string toString = instance.ToString();

                    string typeFullName = configurationValueType.FullName;

                    if (toString.Equals(typeFullName, StringComparison.OrdinalIgnoreCase))
                    {
                        string asJson = JsonConvert.SerializeObject(instance);

                        logItems.Add((instance, $"{typeFullName}: {asJson}"));
                    }
                    else
                    {
                        logItems.Add((instance, $"{typeFullName}: {toString}"));
                    }
                }
            }

            return logItems.ToImmutableArray();
        }
    }
}