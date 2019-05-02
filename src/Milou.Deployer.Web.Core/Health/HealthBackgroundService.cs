using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Startup;

namespace Milou.Deployer.Web.Core.Health
{
    [UsedImplicitly]
    public class HealthBackgroundService : BackgroundService
    {
        private readonly HealthChecker _healthChecker;
        private readonly StartupTaskContext _startupTaskContext;

        public HealthBackgroundService(HealthChecker healthChecker, StartupTaskContext startupTaskContext)
        {
            _healthChecker = healthChecker;
            _startupTaskContext = startupTaskContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!_startupTaskContext.IsCompleted)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }

            await _healthChecker.PerformHealthChecksAsync(stoppingToken);
        }
    }
}
