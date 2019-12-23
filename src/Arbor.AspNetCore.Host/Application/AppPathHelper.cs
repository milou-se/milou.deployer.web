using System;
using System.Collections.Generic;
using System.IO;
using Arbor.App.Extensions.IO;
using Arbor.App.Extensions.Logging;

namespace Arbor.AspNetCore.Host.Application
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
