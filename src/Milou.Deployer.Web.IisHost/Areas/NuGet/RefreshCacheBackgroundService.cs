using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.NuGet
{
    [UsedImplicitly]
    public class RefreshCacheBackgroundService : BackgroundService
    {
        private readonly DeploymentService _deploymentService;
        private readonly ILogger _logger;

        public RefreshCacheBackgroundService(
            [NotNull] DeploymentService deploymentService,
            [NotNull] ILogger logger)
        {
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                try
                {
                    await _deploymentService.GetPackageVersionsAsync(null, false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not update package cache");
                }
            }
        }
    }
}