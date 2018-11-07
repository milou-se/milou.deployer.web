using System;
using System.IO;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Milou.Deployer.Web.Core.Logging
{
    public static class SerilogApiInitialization
    {
        public static ILogger InitializeAppLogging(
            [NotNull] MultiSourceKeyValueConfiguration multiSourceKeyValueConfiguration,
            ILogger logger,
            Action<LoggerConfiguration> loggerConfigurationAction, LoggingLevelSwitch loggingLevelSwitch)
        {
            if (multiSourceKeyValueConfiguration == null)
            {
                throw new ArgumentNullException(nameof(multiSourceKeyValueConfiguration));
            }

            SerilogConfiguration serilogConfiguration =
                multiSourceKeyValueConfiguration.GetInstances<SerilogConfiguration>().FirstOrDefault();
            if (!serilogConfiguration.HasValue())
            {
                logger.Error("Could not get any instance of type {Type}", typeof(SerilogConfiguration));
                return logger;
            }

            if (serilogConfiguration.RollingLogFilePathEnabled && !serilogConfiguration.RollingLogFilePath.HasValue())
            {
                const string message = "Serilog rolling file log path is not set";
                logger.Error(message);
                throw new DeployerAppException(message);
            }

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .Enrich.WithProperty("Application", ApplicationConstants.ApplicationName);

            if (serilogConfiguration.DebugConsoleEnabled)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Debug();
            }

            if (serilogConfiguration.SeqEnabled && serilogConfiguration.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(serilogConfiguration.SeqUrl)
                    && Uri.TryCreate(serilogConfiguration.SeqUrl, UriKind.Absolute, out Uri serilogUrl))
                {
                    logger.Debug("Serilog configured to use Seq with URL {Url}", serilogUrl.AbsoluteUri);
                    loggerConfiguration = loggerConfiguration.WriteTo.Seq(serilogUrl.AbsoluteUri);
                }
            }

            if (serilogConfiguration.RollingLogFilePathEnabled)
            {
                string logFilePath = Path.IsPathRooted(serilogConfiguration.RollingLogFilePath)
                    ? serilogConfiguration.RollingLogFilePath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, serilogConfiguration.RollingLogFilePath);

                var fileInfo = new FileInfo(logFilePath);

                if (fileInfo.Directory != null)
                {
                    string rollingLoggingFile = Path.Combine(fileInfo.Directory.FullName,
                        $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.{{Date}}{Path.GetExtension(fileInfo.Name)}");

                    logger.Debug("Serilog configured to use rolling file with file path {LogFilePath}",
                        rollingLoggingFile);

                    loggerConfiguration = loggerConfiguration
                        .WriteTo.File(rollingLoggingFile);
                }
            }

            loggerConfiguration = loggerConfiguration.WriteTo.Console();

            LoggerConfiguration finalConfiguration = loggerConfiguration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext();

            loggerConfigurationAction?.Invoke(loggerConfiguration);

            Logger appLogger = finalConfiguration
                .CreateLogger();

            appLogger.Debug("Initialized app logging");

            return appLogger;
        }

        public static ILogger InitializeStartupLogging([NotNull] Func<string, string> basePath)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            bool fileLoggingEnabled = bool.TryParse(Environment.GetEnvironmentVariable(LoggingConstants.SerilogStartupLogEnabled),
                         out bool enabled) && enabled;

            string logFile = null;

            if (fileLoggingEnabled)
            {
                string logFilePath = basePath("startup.log");

                Console.WriteLine($"Startup logging is configured to use log file {logFilePath}");

                if (string.IsNullOrWhiteSpace(logFilePath))
                {
                    throw new DeployerAppException("The log path for startup logging is not defined");
                }

                string pathFormat = Environment.ExpandEnvironmentVariables(
                    Environment.GetEnvironmentVariable(LoggingConstants.SerilogStartupLogFilePath) ??
                    logFilePath);

                var fileInfo = new FileInfo(pathFormat);

                if (fileInfo.Directory is null)
                {
                    throw new DeployerAppException("Invalid file directory");
                }

                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                logFile = fileInfo.FullName;
            }

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .WriteTo.Console(LogEventLevel.Verbose);

            if (logFile.HasValue())
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.File(logFile, LogEventLevel.Debug, rollingInterval: RollingInterval.Day);
            }

            string seq = Environment.GetEnvironmentVariable(LoggingConstants.SeqStartupUrl);

            if (!string.IsNullOrWhiteSpace(seq) && Uri.TryCreate(seq, UriKind.Absolute, out Uri _))
            {
                loggerConfiguration.WriteTo.Seq(seq);
            }

            Logger logger = loggerConfiguration
                .CreateLogger();

            logger.Verbose("Startup logging configured");

            return logger;
        }
    }
}
