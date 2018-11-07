using System.Diagnostics;
using System.Reflection;
using Milou.Deployer.Core.Extensions;

namespace Milou.Deployer.Web.Core
{
    public static class VersionHelper
    {
        public static AppVersionInfo GetAppVersion()
        {
            Assembly executingAssembly = typeof(VersionHelper).Assembly.ThrowIfNull();

            AssemblyName assemblyName = executingAssembly.GetName();

            string assemblyVersion = assemblyName.Version.ToString().ThrowIfNullOrEmpty();

            var assemblyInformationalVersionAttribute = executingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            string location = executingAssembly.Location.ThrowIfNullOrEmpty();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(location);

            string fileVersion = fvi.FileVersion;

            return new AppVersionInfo(assemblyVersion, fileVersion, assemblyInformationalVersionAttribute?.InformationalVersion, executingAssembly.FullName);
        }
    }
}