using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.Tooler;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.NuGet;
using Milou.Deployer.Web.Core.Startup;
using Milou.Deployer.Web.Core.Time;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.NuGet
{
    [UsedImplicitly]
    public class NuGetDownloadStartupTask : BackgroundService, IStartupTask
    {
        private readonly IKeyValueConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TimeoutHelper _timeoutHelper;
        private readonly ILogger _logger;
        private readonly NuGetConfiguration _nugetConfiguration;

        public NuGetDownloadStartupTask(
            ILogger logger,
            IKeyValueConfiguration configuration,
            NuGetConfiguration nugetConfiguration,
            IHttpClientFactory httpClientFactory,
            TimeoutHelper timeoutHelper)
        {
            _logger = logger;
            _configuration = configuration;
            _nugetConfiguration = nugetConfiguration;
            _httpClientFactory = httpClientFactory;
            _timeoutHelper = timeoutHelper;
        }

        public bool IsCompleted { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var nugetExePath = "";

            _logger.Debug("Ensuring nuget.exe exists");

            if (!int.TryParse(_configuration[ConfigurationConstants.NuGetDownloadTimeoutInSeconds],
                    out var initialNuGetDownloadTimeoutInSeconds) || initialNuGetDownloadTimeoutInSeconds <= 0)
            {
                initialNuGetDownloadTimeoutInSeconds = 100;
            }

            try
            {
                var fromSeconds = TimeSpan.FromSeconds(initialNuGetDownloadTimeoutInSeconds);

                using (var cts = _timeoutHelper.CreateCancellationTokenSource(fromSeconds))
                {
                    var downloadDirectory = _configuration[ConfigurationConstants.NuGetExeDirectory].WithDefault(null);
                    var exeVersion = _configuration[ConfigurationConstants.NuGetExeVersion].WithDefault(null);

                    var httpClient = _httpClientFactory.CreateClient();

                    var nuGetDownloadClient = new NuGetDownloadClient();
                    var nuGetDownloadResult = await nuGetDownloadClient.DownloadNuGetAsync(
                        new NuGetDownloadSettings(downloadDirectory: downloadDirectory, nugetExeVersion: exeVersion),
                        _logger,
                        httpClient,
                        cts.Token);

                    if (nuGetDownloadResult.Succeeded)
                    {
                        nugetExePath = nuGetDownloadResult.NuGetExePath;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Could not download nuget.exe");
            }

            _nugetConfiguration.NugetExePath = nugetExePath;

            IsCompleted = true;
        }
    }
}
