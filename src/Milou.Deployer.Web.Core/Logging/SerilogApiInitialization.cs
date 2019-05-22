using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.IO;
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
            IEnumerable<ILoggerConfigurationHandler> loggerConfigurationHandlers,
            LoggingLevelSwitch loggingLevelSwitch)
        {
            if (multiSourceKeyValueConfiguration is null)
            {
                throw new ArgumentNullException(nameof(multiSourceKeyValueConfiguration));
            }

            logger.Verbose("Getting Serilog configuration");

            var serilogConfigurations = multiSourceKeyValueConfiguration.GetInstances<SerilogConfiguration>();

            if (serilogConfigurations.Length > 1)
            {
                logger.Warning("Found multiple serilog configurations {Configurations}", serilogConfigurations);
            }

            var serilogConfiguration = serilogConfigurations.FirstOrDefault();

            if (!serilogConfiguration.HasValue())
            {
                logger.Error("Could get Serilog configuration instance");
                return logger;
            }

            if (!serilogConfiguration.IsValid)
            {
                logger.Warning("Serilog app configuration is invalid {Configuration}", serilogConfiguration);
            }
            else
            {
                logger.Debug("Using Serilog app configuration {Configuration}", serilogConfiguration);
            }

            if (serilogConfiguration.RollingLogFilePathEnabled && !serilogConfiguration.RollingLogFilePath.HasValue())
            {
                const string message = "Serilog rolling file log path is not set";
                logger.Error(message);
                throw new DeployerAppException(message);
            }

            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.WithProperty("Application", ApplicationConstants.ApplicationName);

            if (serilogConfiguration.DebugConsoleEnabled)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Debug();
            }

            if (serilogConfiguration.SeqEnabled && serilogConfiguration.IsValid)
            {
                if (serilogConfiguration.SeqUrl.HasValue())
                {
                    logger.Debug("Serilog configured to use Seq with URL {Url}",
                        serilogConfiguration.SeqUrl.AbsoluteUri);
                    loggerConfiguration = loggerConfiguration.WriteTo.Seq(serilogConfiguration.SeqUrl.AbsoluteUri);
                }
                else
                {
                    logger.Debug("Seq not configured for app logging");
                }
            }
            else if (serilogConfiguration.SeqEnabled)
            {
                logger.Debug("Invalid Seq configuration for for app logging");
            }
            else
            {
                logger.Debug("Seq is disabled");
            }

            if (serilogConfiguration.RollingLogFilePathEnabled)
            {
                var logFilePath = Path.IsPathRooted(serilogConfiguration.RollingLogFilePath)
                    ? serilogConfiguration.RollingLogFilePath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, serilogConfiguration.RollingLogFilePath);

                var fileInfo = new FileInfo(logFilePath);

                if (fileInfo.Directory != null)
                {
                    fileInfo.Directory.EnsureExists();
                    var rollingLoggingFile = Path.Combine(fileInfo.Directory.FullName,
                        $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}{Path.GetExtension(fileInfo.Name)}");

                    logger.Debug("Serilog configured to use rolling file with file path {LogFilePath}",
                        rollingLoggingFile);

                    loggerConfiguration = loggerConfiguration
                        .WriteTo.File(rollingLoggingFile, rollingInterval: RollingInterval.Day);
                }
                else
                {
                    logger.Warning("Log file directory is null");
                }
            }
            else
            {
                logger.Debug("Rolling file log is disabled");
            }

            loggerConfiguration = loggerConfiguration.WriteTo.Console();

            var microsoftLevel =
                multiSourceKeyValueConfiguration[LoggingConstants.MicrosoftLevel].ParseOrDefault(LogEventLevel.Warning);

            var finalConfiguration = loggerConfiguration
                .MinimumLevel.Override("Microsoft", microsoftLevel)
                .Enrich.FromLogContext();

            foreach (var loggerConfigurationHandler in loggerConfigurationHandlers)
            {
                logger.Debug("Running logger configuration handler {Handler}", loggerConfigurationHandler.GetType().FullName);
                loggerConfiguration = loggerConfigurationHandler.Handle(loggerConfiguration);
            }

            logger.Debug("App logging current switch level is set to {Level}", loggingLevelSwitch.MinimumLevel);

            var appLogger = finalConfiguration
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .CreateLogger();

            appLogger.Debug("Initialized app logging");

            return appLogger;
        }

        public static ILogger InitializeStartupLogging(
            [NotNull] Func<string, string> basePath,
            IReadOnlyDictionary<string, string> environmentVariables,
            IEnumerable<IStartupLoggerConfigurationHandler> startupLoggerConfigurationHandlers)
        {
            var startupLevel = LogEventLevel.Verbose;

            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            var fileLoggingEnabled = bool.TryParse(
                                         environmentVariables.ValueOrDefault(LoggingConstants.SerilogStartupLogEnabled),
                                         out var enabled) && enabled;

            string logFile = null;

            if (fileLoggingEnabled)
            {
                var logFilePath = basePath("startup.log");

                TempLogger.WriteLine($"Startup logging is configured to use log file {logFilePath}");

                if (string.IsNullOrWhiteSpace(logFilePath))
                {
                    throw new DeployerAppException("The log path for startup logging is not defined");
                }

                var pathFormat = Environment.ExpandEnvironmentVariables(
                    environmentVariables.ValueOrDefault(LoggingConstants.SerilogStartupLogFilePath) ??
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

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(startupLevel)
                .WriteTo.Console(startupLevel);

            if (logFile.HasValue())
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.File(logFile, startupLevel, rollingInterval: RollingInterval.Day);
            }

            var seq = environmentVariables.ValueOrDefault(LoggingConstants.SeqStartupUrl);

            Uri usedSeqUri = null;
            if (!string.IsNullOrWhiteSpace(seq))
            {
                var seqUrl = Environment.ExpandEnvironmentVariables(seq);

                if (Uri.TryCreate(seqUrl, UriKind.Absolute, out var uri))
                {
                    usedSeqUri = uri;
                    loggerConfiguration.WriteTo.Seq(seqUrl).MinimumLevel.Is(startupLevel);
                }
            }

            foreach (var startupLoggerConfigurationHandler in startupLoggerConfigurationHandlers)
            {
                loggerConfiguration = startupLoggerConfigurationHandler.Handle(loggerConfiguration);
            }

            var logger = loggerConfiguration.CreateLogger();

            TempLogger.FlushWith(logger);

            logger.Verbose("Startup logging configured, minimum log level {LogLevel}, seq {Seq}",
                startupLevel,
                usedSeqUri);

            logger.Information("Using application root directory {Directory}", basePath(""));

            return logger;
        }
    }
}
