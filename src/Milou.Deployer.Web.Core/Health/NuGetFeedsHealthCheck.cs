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
        private readonly NuGetDownloadClient _nuGetDownloadClient;

        public NuGetFeedsHealthCheck(
            [NotNull] IHttpClientFactory httpClient,
            [NotNull] ILogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nuGetDownloadClient = new NuGetDownloadClient();
        }

        public int TimeoutInSeconds { get; } = 7;

        public string Description { get; } = "NuGet feed check";

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
        {
            NuGetDownloadResult nuGetDownloadResult = await _nuGetDownloadClient.DownloadNuGetAsync(
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

            ConcurrentDictionary<Uri, bool?> nugetFeeds = GetFeedUrls(lines);

            List<Task> tasks = nugetFeeds.Keys
                .Select(nugetFeed => CheckFeedAsync(cancellationToken, nugetFeed, nugetFeeds))
                .ToList();

            await Task.WhenAll(tasks);

            bool allSucceeded = nugetFeeds.All(pair => pair.Value == true);

            return new HealthCheckResult(allSucceeded);
        }

        private ConcurrentDictionary<Uri, bool?> GetFeedUrls(List<string> lines)
        {
            var nugetFeeds = new ConcurrentDictionary<Uri, bool?>();

            for (int i = 0; i < lines.Count; i++)
            {
                ReadOnlySpan<char> line = lines[i].AsSpan();

                if (line.IsEmpty)
                {
                    continue;
                }

                if (line[0] != 'E' && line[0] != 'e')
                {
                    continue;
                }

                int firstSpace = line.IndexOf(' ');

                if (firstSpace < 0)
                {
                    continue;
                }

                string url = line.Slice(firstSpace + 1).ToString();

                if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                {
                    continue;
                }

                nugetFeeds.TryAdd(uri, null);
            }

            return nugetFeeds;
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