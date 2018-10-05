using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class Assemblies
    {
        public static ImmutableArray<Assembly> FilteredAssemblies(
            string assemblyNameStartsWith = null,
            bool useCache = true)
        {
            return AppDomain.CurrentDomain.FilteredAssemblies(assemblyNameStartsWith, useCache);
        }
    }
}