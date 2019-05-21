﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core.Extensions.BoolExtensions;
using Microsoft.AspNetCore.Hosting;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Cli;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Logging;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Serilog;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost
{
    public static class AppStarter
    {
        public static async Task<int> StartAsync(
            string[] args,
            IReadOnlyDictionary<string, string> environmentVariables,
            params object[] instances)
        {
            try
            {
                if (args is null)
                {
                    args = Array.Empty<string>();
                }

                if (args.Length > 0)
                {
                    TempLogger.WriteLine("Started with arguments:");
                    foreach (var arg in args)
                    {
                        TempLogger.WriteLine(arg);
                    }
                }

                CancellationTokenSource cancellationTokenSource;

                if (int.TryParse(
                        environmentVariables.GetValueOrDefault(ConfigurationConstants.RestartTimeInSeconds),
                        out var intervalInSeconds) && intervalInSeconds > 0)
                {
                    cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(intervalInSeconds));
                }
                else
                {
                    cancellationTokenSource = new CancellationTokenSource();
                }

                using (cancellationTokenSource)
                {
                    cancellationTokenSource.Token.Register(
                        () => TempLogger.WriteLine("App cancellation token triggered"));

                    using (var app = await App.CreateAsync(cancellationTokenSource, args, environmentVariables, instances))
                    {
                        var runAsService = app.Configuration.ValueOrDefault(ApplicationConstants.RunAsService)
                                           && !Debugger.IsAttached;

                        app.Logger.Information("Starting application {Application}", app.AppInstance);

                        if (intervalInSeconds > 0)
                        {
                            app.Logger.Debug(
                                "Restart time is set to {RestartIntervalInSeconds} seconds for {App}",
                                intervalInSeconds,
                                app.AppInstance);
                        }
                        else if (app.Logger.IsEnabled(LogEventLevel.Verbose))
                        {
                            app.Logger.Verbose("Restart time is disabled");
                        }

                        string[] runArgs;

                        if (!args.Contains(ApplicationConstants.RunAsService) && runAsService)
                        {
                            runArgs = args
                                .Concat(new[] { ApplicationConstants.RunAsService })
                                .ToArray();
                        }
                        else
                        {
                            runArgs = args;
                        }

                        await app.RunAsync(runArgs);

                        if (!runAsService)
                        {
                            app.Logger.Debug("Started {App}, waiting for web host shutdown", app.AppInstance);

                            await app.WebHost.WaitForShutdownAsync(cancellationTokenSource.Token);
                        }

                        app.Logger.Information(
                            "Stopping application {Application}",
                            app.AppInstance);
                    }
                }

                if (int.TryParse(
                        environmentVariables.GetValueOrDefault(ConfigurationConstants.ShutdownTimeInSeconds),
                        out var shutDownTimeInSeconds) && shutDownTimeInSeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(shutDownTimeInSeconds), CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(2000));

                var exceptionLogDirectory = args?.ParseParameter("exceptionDir");

                var loggerConfiguration = new LoggerConfiguration()
                    .WriteTo.File(Path.Combine(exceptionLogDirectory ?? AppDomain.CurrentDomain.BaseDirectory, "Exception.log"))
                    .MinimumLevel.Verbose();

                if (environmentVariables.TryGetValue(LoggingConstants.SeqStartupUrl, out string url))
                {
                    loggerConfiguration = loggerConfiguration.WriteTo.Seq(url);
                }

                var logger = loggerConfiguration.CreateLogger();

                using (logger)
                {
                    logger.Fatal(ex, "Could not start application");
                    TempLogger.FlushWith(logger);

                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));

                return 1;
            }

            return 0;
        }
    }
}
