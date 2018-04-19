using System;
using System.IO;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Logging
{
    public static class SerilogApiInitialization
    {
        public static void InitializeAppLogging(
            [NotNull] MultiSourceKeyValueConfiguration multiSourceKeyValueConfiguration, Func<LoggerConfiguration, LoggerConfiguration> interceptor = null)
        {
            if (multiSourceKeyValueConfiguration == null)
            {
                throw new ArgumentNullException(nameof(multiSourceKeyValueConfiguration));
            }

            var serilogConfiguration =
                multiSourceKeyValueConfiguration.GetInstance<SerilogConfiguration>();

            if (!serilogConfiguration.HasValue())
            {
                Log.Logger.Error("Could not get any instance of type {Type}", typeof(SerilogConfiguration));
                return;
            }

            if (serilogConfiguration.RollingLogFilePathEnabled && !serilogConfiguration.RollingLogFilePath.HasValue())
            {
                const string message = "Serilog rolling file log path is not set";
                Log.Logger.Error(message);
                throw new InvalidOperationException(message);
            }

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug);

            bool seqEnabled = false;

            if (serilogConfiguration.SeqEnabled
                && serilogConfiguration.IsValid
                && serilogConfiguration.SeqUrl.HasValue())
            {
                Log.Logger.Information("Serilog configured to use Seq with URL {Url}", serilogConfiguration.SeqUrl);
                loggerConfiguration = loggerConfiguration.WriteTo.Seq(serilogConfiguration.SeqUrl.ToString());

                seqEnabled = true;
            }

            if (serilogConfiguration.RollingLogFilePathEnabled)
            {
                string logFilePath = Path.IsPathRooted(serilogConfiguration.RollingLogFilePath)
                    ? serilogConfiguration.RollingLogFilePath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, serilogConfiguration.RollingLogFilePath);

                var fileInfo = new FileInfo(logFilePath);

                string rollingLoggingFile = Path.Combine(fileInfo.FullName);

                Log.Logger.Information("Serilog configured to use rolling file with file path {LogFilePath}",
                    rollingLoggingFile);

                loggerConfiguration = loggerConfiguration
                    .WriteTo.File(rollingLoggingFile, rollingInterval: RollingInterval.Day);
            }

            if (seqEnabled)
            {
                Log.Logger.Information("Serilog configured to use Seq with URL {Url}", serilogConfiguration.SeqUrl);
            }

            if (serilogConfiguration.ConsoleEnabled)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Console();
            }

            if (interceptor != null)
            {
                loggerConfiguration = interceptor.Invoke(loggerConfiguration);
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            Log.Logger.Information("Initialized app logging");
        }

        public static void InitializeStartupLogging([NotNull] Func<string, string> basePath)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            string logFilePath = basePath("startup.log");

            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                throw new InvalidOperationException("The log path for startup logging is not defined");
            }

            string pathFormat =
                (Environment.GetEnvironmentVariable(LoggingConstants.SerilogStartupLogFilePath) ??
                logFilePath).WithDefault(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log"));

            var fileInfo = new FileInfo(pathFormat);

            if (!fileInfo.Directory?.Exists ?? false)
            {
                fileInfo.Directory.Create();
            }

            string rollingLoggingFile = Path.Combine(fileInfo.Directory.FullName,
                $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.{{Date}}{Path.GetExtension(fileInfo.Name)}");

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console(LogEventLevel.Debug)
                .WriteTo.File(rollingLoggingFile, LogEventLevel.Debug);

            Log.Logger = loggerConfiguration
                .CreateLogger();

            Log.Logger.Information("Starting app");
        }
    }
}