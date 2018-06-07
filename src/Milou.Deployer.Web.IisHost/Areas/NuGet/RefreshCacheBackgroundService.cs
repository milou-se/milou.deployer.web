using System;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core.Metadata;
using Arbor.KVConfiguration.Urns;
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
        private readonly NuGetCacheConfiguration _nugetCacheConfiguration;

        public RefreshCacheBackgroundService(
            [NotNull] DeploymentService deploymentService,
            [NotNull] ILogger logger,
            NuGetCacheConfiguration nugetCacheConfiguration)
        {
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nugetCacheConfiguration = nugetCacheConfiguration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await Task.Delay(TimeSpan.FromSeconds(_nugetCacheConfiguration.StartupDelayInSeconds), stoppingToken);

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    try
            //    {
            //        await _deploymentService.GetPackageVersionsAsync(null, false, _logger, cancellationToken: stoppingToken);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.Error(ex, "Could not update package cache");
            //    }

            //    await Task.Delay(TimeSpan.FromSeconds(_nugetCacheConfiguration.CacheRefreshIntervalInSeconds), stoppingToken);
            //}
        }
    }

    [Urn(NuGetCacheConstants.Configuration)]
    [UsedImplicitly]
    public class NuGetCacheConfiguration
    {
        private const int DefaultCacheRefreshIntervalInSeconds = 180;

        private const int DefaultCacheRefreshStartupDelayInSeconds = 120;

        public int CacheRefreshIntervalInSeconds { get; }

        public int StartupDelayInSeconds { get; }

        public NuGetCacheConfiguration(int cacheRefreshIntervalInSeconds = DefaultCacheRefreshIntervalInSeconds, int startupDelayInSeconds = DefaultCacheRefreshStartupDelayInSeconds)
        {
            CacheRefreshIntervalInSeconds = cacheRefreshIntervalInSeconds <= 0 ? DefaultCacheRefreshIntervalInSeconds : cacheRefreshIntervalInSeconds;
            StartupDelayInSeconds = startupDelayInSeconds <= 0 ? DefaultCacheRefreshStartupDelayInSeconds : startupDelayInSeconds;
        }
    }

    public static class NuGetCacheConstants
    {
        public const string Configuration = "urn:milou:deployer:web:nuget:cache:configuration";

        [Metadata(defaultValue: "true")]
        public const string Enabled = "urn:milou:deployer:web:nuget:cache:configuration:default:enabled";
    }
}