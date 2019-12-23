using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Milou.Deployer.Web.Core.IO;
using Milou.Deployer.Web.Core.Logging;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class AppPathHelper
    {
        public static void SetApplicationPaths(ApplicationPaths paths, IReadOnlyCollection<string> commandLineArgs)
        {
            var currentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (WindowsServiceHelper.IsRunningAsService(commandLineArgs))
            {
                TempLogger.WriteLine(
                    $"Switching current directory from {Directory.GetCurrentDirectory()} to {currentDomainBaseDirectory}");
                Directory.SetCurrentDirectory(currentDomainBaseDirectory);
            }

            paths.BasePath = paths.BasePath ?? currentDomainBaseDirectory;
            paths.ContentBasePath = paths.ContentBasePath ?? Directory.GetCurrentDirectory();
        }
    }
}
