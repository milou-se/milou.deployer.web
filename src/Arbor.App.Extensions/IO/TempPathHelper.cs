using System;
using Arbor.App.Extensions.Application;
using Arbor.KVConfiguration.Core;
using Serilog;

namespace Arbor.App.Extensions.IO
{
    public static class TempPathHelper
    {
        public static void SetTempPath(MultiSourceKeyValueConfiguration configuration, ILogger startupLogger)
        {
            string tempDirectory = configuration[ApplicationConstants.ApplicationTempDirectory];

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