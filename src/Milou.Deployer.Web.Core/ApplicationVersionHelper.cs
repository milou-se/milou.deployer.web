using System.Diagnostics;
using System.Reflection;
using Milou.Deployer.Core.Extensions;

namespace Milou.Deployer.Web.Core
{
    public static class ApplicationVersionHelper
    {
        public static ApplicationVersionInfo GetAppVersion()
        {
            Assembly executingAssembly = typeof(ApplicationVersionHelper).Assembly.ThrowIfNull();

            AssemblyName assemblyName = executingAssembly.GetName();

            string assemblyVersion = assemblyName.Version.ToString().ThrowIfNullOrEmpty();

            var assemblyInformationalVersionAttribute = executingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            string location = executingAssembly.Location.ThrowIfNullOrEmpty();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(location);

            string fileVersion = fvi.FileVersion;

            return new ApplicationVersionInfo(assemblyVersion, fileVersion, assemblyInformationalVersionAttribute?.InformationalVersion, executingAssembly.FullName);
        }
    }
}