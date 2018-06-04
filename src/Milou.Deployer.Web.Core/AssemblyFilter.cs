using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core
{
    public static class AssemblyFilter
    {
        private static readonly ImmutableArray<string> _WhiteListed = new[] { "milou" }.ToImmutableArray();

        public static bool FilterAssemblies([NotNull] Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (assembly.IsDynamic)
            {
                return false;
            }

            string assemblyName = assembly.GetName().Name;

            bool isIncluded = _WhiteListed.Any(whiteListed =>
                assemblyName.StartsWith(whiteListed, StringComparison.OrdinalIgnoreCase));

            return isIncluded;
        }
    }
}