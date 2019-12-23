using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Arbor.App.Extensions.Application;

namespace Arbor.AspNetCore.Host
{
    public static class WindowsServiceHelper
    {
        public static bool IsRunningAsService(IReadOnlyCollection<string> commandLineArgs)
        {
            bool hasRunAsServiceArgument = commandLineArgs.Any(arg =>
                arg.Equals(ApplicationConstants.RunAsService, StringComparison.OrdinalIgnoreCase));

            if (hasRunAsServiceArgument)
            {
                return true;
            }

            FileInfo processFileInfo;
            using (var currentProcess = Process.GetCurrentProcess())
            {
                if (currentProcess.MainModule is null)
                {
                    throw new InvalidOperationException("The main module for the current process could not be found");
                }

                processFileInfo = new FileInfo(currentProcess.MainModule.FileName);
            }

            if (processFileInfo.Name.Equals("Milou.Deployer.Web.WindowsService.exe",
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}