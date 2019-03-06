using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Milou.Deployer.Web.Core.Health
{
    [UsedImplicitly]
    public class HealthBackgroundService : BackgroundService
    {
        private readonly HealthChecker _healthChecker;

        public HealthBackgroundService(HealthChecker healthChecker)
        {
            _healthChecker = healthChecker;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _healthChecker.PerformHealthChecksAsync(stoppingToken);
        }
    }
}
