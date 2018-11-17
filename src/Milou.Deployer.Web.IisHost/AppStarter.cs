using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Core.Extensions.BoolExtensions;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost
{
    public static class AppStarter
    {
        public static async Task<int> StartAsync(string[] args)
        {
            if (args is null)
            {
                args = Array.Empty<string>();
            }

            CancellationTokenSource cancellationTokenSource;

            if (int.TryParse(Environment.GetEnvironmentVariable(ConfigurationConstants.RestartTimeInSeconds),
                    out int intervalInSeconds) && intervalInSeconds > 0)
            {
                cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(intervalInSeconds));
            }
            else
            {
                cancellationTokenSource = new CancellationTokenSource();
            }

            using (cancellationTokenSource)
            {
                cancellationTokenSource.Token.Register(() => Console.WriteLine("App cancellation token triggered"));

                using (App app = await App.CreateAsync(cancellationTokenSource, null, args))
                {
                    bool runAsService = app.AppRootScope.Deepest().Lifetime.Resolve<IKeyValueConfiguration>().ValueOrDefault(ApplicationConstants.RunAsService) && !Debugger.IsAttached;

                    app.Logger.Information("Starting application {Application}", app.AppInstance);

                    if (intervalInSeconds > 0)
                    {
                        app.Logger.Debug("Restart time is set to {RestartIntervalInSeconds} seconds for {App}",
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

                    app.Logger.Information("Stopping application {Application}",
                        app.AppInstance);
                }
            }

            if (int.TryParse(Environment.GetEnvironmentVariable(ConfigurationConstants.ShutdownTimeInSeconds),
                    out int shutDownTimeInSeconds) && shutDownTimeInSeconds > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(shutDownTimeInSeconds), CancellationToken.None);
            }

            return 0;
        }
    }
}