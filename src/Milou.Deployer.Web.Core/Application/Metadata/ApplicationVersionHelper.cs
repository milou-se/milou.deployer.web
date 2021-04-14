using System.Diagnostics;
using System.Reflection;
using Arbor.App.Extensions.Application;
using Milou.Deployer.Core.Extensions;

namespace Milou.Deployer.Web.Core.Application.Metadata
{
    public static class ApplicationVersionHelper
    {
        public static ApplicationVersionInfo GetAppVersion()
        {
            var executingAssembly = typeof(ApplicationVersionHelper).Assembly.ThrowIfNull();

            var assemblyName = executingAssembly.GetName();

            string assemblyVersion = assemblyName.Version?.ToString()?.ThrowIfNullOrEmpty();

            var assemblyInformationalVersionAttribute =
                executingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            string location = executingAssembly.Location.ThrowIfNullOrEmpty();

            var fvi = FileVersionInfo.GetVersionInfo(location);

            string fileVersion = fvi.FileVersion;

            return new ApplicationVersionInfo(assemblyVersion,
                fileVersion,
                assemblyInformationalVersionAttribute?.InformationalVersion ?? fileVersion,
                executingAssembly.FullName);
        }
    }
}