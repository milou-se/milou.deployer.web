using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Arbor.AspNetCore.Host.Configuration
{
    [UsedImplicitly]
    public class ConfigurationBackgroundService : BackgroundService
    {
        private readonly UserConfigUpdater _updater;

        public ConfigurationBackgroundService(UserConfigUpdater updater) => _updater = updater;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _updater.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
