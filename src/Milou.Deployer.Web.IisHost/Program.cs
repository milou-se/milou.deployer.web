using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Areas.Configuration;

namespace Milou.Deployer.Web.IisHost
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
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
                    app.Logger.Information("Starting application {Application}", ApplicationConstants.ApplicationName);

                    app.Logger.Debug("Restart time is set to {RestartIntervalInSeconds} seconds", intervalInSeconds);

                    await app.RunAsync(args);

                    app.Logger.Debug("Started Milou Deployer Web app, waiting for web host shutdown");

                    await app.WebHost.WaitForShutdownAsync(cancellationTokenSource.Token);

                    app.Logger.Information("Stopping application {Application}", ApplicationConstants.ApplicationName);
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(3000), CancellationToken.None);

            return 0;
        }
    }
}
