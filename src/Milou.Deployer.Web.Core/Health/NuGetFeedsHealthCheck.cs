using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.Tooler;
using JetBrains.Annotations;
using Milou.Deployer.Core.Processes;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.Core.Health
{
    [UsedImplicitly]
    public class NuGetFeedsHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger _logger;

        public NuGetFeedsHealthCheck(
            [NotNull] IHttpClientFactory httpClient,
            [NotNull] ILogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int TimeoutInSeconds { get; } = 7;

        public string Description { get; } = "NuGet feed check";

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
        {
            NuGetDownloadResult nuGetDownloadResult = await new NuGetDownloadClient().DownloadNuGetAsync(
                NuGetDownloadSettings.Default,
                _logger,
                _httpClient.CreateClient("NuGet"),
                cancellationToken);

            if (!nuGetDownloadResult.Succeeded)
            {
                return new HealthCheckResult(false);
            }

            var args = new List<string>
            {
                "Sources",
                "-Format",
                "Short"
            };

            var lines = new List<string>();

            ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(nuGetDownloadResult.NuGetExePath,
                args,
                (message, _) =>
                {
                    lines.Add(message);
                    _logger.Verbose("{Message}", message);
                },
                (message, category) => _logger.Verbose("{Category}{Message}",category, message),
                (message, category) => _logger.Verbose("{Category}{Message}",category, message),
                debugAction: (message, category) => _logger.Verbose("{Category}{Message}",category, message),
                cancellationToken: cancellationToken);

            if (!exitCode.IsSuccess)
            {
                return new HealthCheckResult(false);
            }

            var nugetFeeds = new ConcurrentDictionary<Uri, bool?>();

            foreach (string line in lines.Where(l => l.StartsWith("E", StringComparison.Ordinal)))
            {
                int firstSpace = line.IndexOf(' ');

                if (firstSpace < 0)
                {
                    continue;
                }

                string url = line.Substring(firstSpace).Trim();

                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                {
                    continue;
                }

                if (!uri.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                nugetFeeds.TryAdd(uri, null);
            }

            var tasks = new List<Task>();

            foreach (Uri nugetFeed in nugetFeeds.Keys)
            {
                tasks.Add(CheckFeedAsync(cancellationToken, nugetFeed, nugetFeeds));
            }

            await Task.WhenAll(tasks);

            bool allSucceeded = nugetFeeds.All(pair => pair.Value == true);

            return new HealthCheckResult(allSucceeded);
        }

        private async Task CheckFeedAsync(
            CancellationToken cancellationToken,
            Uri nugetFeed,
            ConcurrentDictionary<Uri, bool?> nugetFeeds)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, nugetFeed))
                {
                    using (HttpResponseMessage httpResponseMessage = await _httpClient.CreateClient(nugetFeed.Host)
                        .SendAsync(request, cancellationToken))
                    {
                        if (httpResponseMessage.StatusCode == HttpStatusCode.OK ||
                            httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            nugetFeeds[nugetFeed] = true;
                        }
                        else
                        {
                            nugetFeeds[nugetFeed] = false;
                            _logger.Verbose(
                                "Failed to get expected result from NuGet feed {Feed}, status code {HttpStatusCode}",
                                nugetFeed,
                                httpResponseMessage.StatusCode);
                        }
                    }
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Verbose(ex, "Could not get {Uri}", nugetFeed);
            }
        }
    }
}