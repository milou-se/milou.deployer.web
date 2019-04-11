using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.Processing;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.NuGet;
using Serilog;

namespace Milou.Deployer.Web.Core.Health
{
    [UsedImplicitly]
    public class NuGetFeedsHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger _logger;
        private readonly NuGetConfiguration _nuGetConfiguration;

        public NuGetFeedsHealthCheck(
            [NotNull] IHttpClientFactory httpClient,
            [NotNull] ILogger logger,
            [NotNull] NuGetConfiguration nuGetConfiguration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nuGetConfiguration = nuGetConfiguration ?? throw new ArgumentNullException(nameof(nuGetConfiguration));
        }

        private ConcurrentDictionary<Uri, bool?> GetFeedUrls(List<string> lines)
        {
            var nugetFeeds = new ConcurrentDictionary<Uri, bool?>();

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i].AsSpan();

                if (line.IsEmpty)
                {
                    continue;
                }

                if (line[0] != 'E' && line[0] != 'e')
                {
                    continue;
                }

                var firstSpace = line.IndexOf(' ');

                if (firstSpace < 0)
                {
                    continue;
                }

                var url = line.Slice(firstSpace + 1).ToString();

                if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
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
                    using (var httpResponseMessage = await _httpClient.CreateClient(nugetFeed.Host)
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

        public int TimeoutInSeconds { get; } = 7;

        public string Description { get; } = "NuGet feed check";

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_nuGetConfiguration.NugetExePath) ||
                !File.Exists(_nuGetConfiguration.NugetExePath))
            {
                _logger.Warning("Could not perform health checks of NuGet feeds, nuget.exe is missing");
                return new HealthCheckResult(false);
            }

            var args = new List<string>
            {
                "Sources",
                "-Format",
                "Short"
            };

            var lines = new List<string>();

            var exitCode = await ProcessRunner.ExecuteProcessAsync(_nuGetConfiguration.NugetExePath,
                args,
                (message, _) =>
                {
                    lines.Add(message);
                    _logger.Verbose("{Message}", message);
                },
                (message, category) => _logger.Verbose("{Category}{Message}", category, message),
                (message, category) => _logger.Verbose("{Category}{Message}", category, message),
                debugAction: (message, category) => _logger.Verbose("{Category}{Message}", category, message),
                cancellationToken: cancellationToken);

            if (!exitCode.IsSuccess)
            {
                return new HealthCheckResult(false);
            }

            var nugetFeeds = GetFeedUrls(lines);

            var tasks = nugetFeeds.Keys
                .Select(nugetFeed => CheckFeedAsync(cancellationToken, nugetFeed, nugetFeeds))
                .ToList();

            await Task.WhenAll(tasks);

            var allSucceeded = nugetFeeds.All(pair => pair.Value == true);

            return new HealthCheckResult(allSucceeded);
        }
    }
}
