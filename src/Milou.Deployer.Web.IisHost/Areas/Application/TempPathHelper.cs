using System;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class TempPathHelper
    {
        public static void SetTempPath(MultiSourceKeyValueConfiguration configuration, ILogger startupLogger)
        {
            var tempDirectory = configuration[ApplicationConstants.ApplicationTempDirectory];

            if (!string.IsNullOrWhiteSpace(tempDirectory))
            {
                if (tempDirectory.TryEnsureDirectoryExists(out var tempDirectoryInfo))
                {
                    Environment.SetEnvironmentVariable(TempConstants.Tmp, tempDirectoryInfo.FullName);
                    Environment.SetEnvironmentVariable(TempConstants.Temp, tempDirectoryInfo.FullName);

                    startupLogger.Debug("Using specified temp directory {TempDirectory} {AppName}",
                        tempDirectory,
                        ApplicationConstants.ApplicationName);
                }
                else
                {
                    startupLogger.Warning("Could not use specified temp directory {TempDirectory}, {AppName}",
                        tempDirectory,
                        ApplicationConstants.ApplicationName);
                }
            }
        }
    }
}
