using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Schema.Json;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application.Metadata;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Packages;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Time;
using Milou.Deployer.Web.IisHost.Areas.NuGet;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [UsedImplicitly]
    public class MonitoringService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        [NotNull]
        private readonly MonitorConfiguration _monitorConfiguration;

        private readonly PackageService _packageService;

        private readonly TimeoutHelper _timeoutHelper;
        private readonly NuGetListConfiguration _nuGetListConfiguration;

        public MonitoringService(
            [NotNull] ILogger logger,
            [NotNull] IHttpClientFactory httpClientFactory,
            [NotNull] PackageService packageService,
            [NotNull] MonitorConfiguration monitorConfiguration,
            TimeoutHelper timeoutHelper,
            NuGetListConfiguration nuGetListConfiguration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
            _monitorConfiguration =
                monitorConfiguration ?? throw new ArgumentNullException(nameof(monitorConfiguration));
            _timeoutHelper = timeoutHelper;
            _nuGetListConfiguration = nuGetListConfiguration;
        }

        private static async Task<AppVersion> GetAppVersionAsync(
            HttpResponseMessage response,
            DeploymentTarget target,
            IReadOnlyCollection<PackageVersion> filtered,
            CancellationToken cancellationToken)
        {
            if (response.Content.Headers.ContentType?.MediaType.Equals("application/json",
                    StringComparison.OrdinalIgnoreCase) != true)
            {
                return new AppVersion(target, "Response not JSON", filtered);
            }

            string json = await response.Content.ReadAsStringAsync();

            if (cancellationToken.IsCancellationRequested)
            {
                return new AppVersion(target, "Timeout", filtered);
            }

            var configuration =
                JsonConfigurationSerializer.Deserialize(json);

            var nameValueCollection = new NameValueCollection();

            if (!configuration.Keys.IsDefaultOrEmpty)
            {
                foreach (var configurationItem in configuration.Keys)
                {
                    nameValueCollection.Add(configurationItem.Key, configurationItem.Value);
                }
            }

            var appVersion = new AppVersion(target, new InMemoryKeyValueConfiguration(nameValueCollection), filtered);

            return appVersion;
        }

        private async Task<IReadOnlyCollection<PackageVersion>> GetAllowedPackagesAsync(
            DeploymentTarget target,
            CancellationToken cancellationToken)
        {
            CancellationTokenSource cancellationTokenSource;
            CancellationTokenSource linkedCancellationTokenSource = null;
            if (target.PackageListTimeout.HasValue)
            {
                cancellationTokenSource = new CancellationTokenSource(target.PackageListTimeout.Value);
                cancellationToken = cancellationTokenSource.Token;
            }
            else
            {
                cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(_nuGetListConfiguration.ListTimeOutInSeconds));

                linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
                cancellationToken = linkedCancellationTokenSource.Token;
            }

            try
            {
                var allPackageVersions =
                    await _packageService.GetPackageVersionsAsync(
                        target.PackageId,
                        nugetConfigFile: target.NuGetConfigFile,
                        nugetPackageSource: target.NuGetPackageSource,
                        logger: _logger,
                        includePreReleased: target.AllowExplicitExplicitPreRelease == true || target.AllowPreRelease,
                        cancellationToken: cancellationToken);

                var allTargetPackageVersions = allPackageVersions.Where(
                        packageVersion =>
                            target.PackageId.Equals(packageVersion.PackageId,
                                StringComparison.OrdinalIgnoreCase))
                    .SafeToReadOnlyCollection();

                var preReleaseFiltered = allTargetPackageVersions;

                if (!target.AllowPreRelease)
                {
                    preReleaseFiltered =
                        allTargetPackageVersions.Where(package => !package.Version.IsPrerelease)
                            .SafeToReadOnlyCollection();
                }

                var filtered =
                    preReleaseFiltered.OrderByDescending(packageVersion => packageVersion.Version)
                        .SafeToReadOnlyCollection();

                return filtered;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Error(ex, "Could not get allowed packages for target {Target}", target.Id);
                return ImmutableArray<PackageVersion>.Empty;
            }
            finally
            {
                cancellationTokenSource.Dispose();
                linkedCancellationTokenSource?.Dispose();
            }
        }

        private Task<(HttpResponseMessage, string)> GetApplicationMetadataTask(
            DeploymentTarget deploymentTarget,
            CancellationToken cancellationToken)
        {
            var uriBuilder = new UriBuilder(deploymentTarget.Url)
            {
                Path = "applicationmetadata.json"
            };

            var applicationMetadataUri = uriBuilder.Uri;

            var getApplicationMetadataTask = GetWrappedResponseAsync(
                applicationMetadataUri,
                cancellationToken);
            return getApplicationMetadataTask;
        }

        private async Task<(HttpResponseMessage, string)> GetWrappedResponseAsync(
            Uri applicationMetadataUri,
            CancellationToken cancellationToken)
        {
            _logger.Debug("Making metadata request to {RequestUri}", applicationMetadataUri);

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                var client = _httpClientFactory.CreateClient(applicationMetadataUri.Host);
                var wrappedResponseAsync = (await client.GetAsync(applicationMetadataUri, cancellationToken), "");
                stopwatch.Stop();
                _logger.Debug("Metadata call to {Url} took {Elapsed} milliseconds", applicationMetadataUri, stopwatch.ElapsedMilliseconds);

                return wrappedResponseAsync;
            }
            catch (TaskCanceledException ex)
            {
                _logger.Error(ex,
                    "Could not get application metadata for {ApplicationMetadataUri}",
                    applicationMetadataUri);
                return ((HttpResponseMessage)null, "Timeout");
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Error(ex,
                    "Could not get application metadata for {ApplicationMetadataUri}",
                    applicationMetadataUri);
                return ((HttpResponseMessage)null, ex.Message);
            }
        }

        public async Task<AppVersion> GetAppMetadataAsync(
            [NotNull] DeploymentTarget target,
            CancellationToken cancellationToken)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            AppVersion appMetadata;
            using (var cancellationTokenSource =
                _timeoutHelper.CreateCancellationTokenSource(TimeSpan.FromSeconds(_monitorConfiguration.DefaultTimeoutInSeconds)))
            {
                if (_logger.IsEnabled(LogEventLevel.Verbose))
                {
                    cancellationTokenSource.Token.Register(() =>
                    {
                        _logger.Verbose("{Method} for {Target}, cancellation token invoked out after {Seconds} seconds",
                            nameof(GetAppMetadataAsync),
                            target,
                            _monitorConfiguration.DefaultTimeoutInSeconds);
                    });
                }

                using (var linkedTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token))
                {
                    var metadataTask = GetApplicationMetadataTask(target, linkedTokenSource.Token);

                    var allowedPackagesTask = GetAllowedPackagesAsync(target, linkedTokenSource.Token);

                    await Task.WhenAll(metadataTask, allowedPackagesTask);

                    var (response, message) = await metadataTask;

                    var packages = await allowedPackagesTask;

                    using (var httpResponseMessage = response)
                    {
                        if (httpResponseMessage == null)
                        {
                            return new AppVersion(target,
                                message ?? $"Could not get application metadata from target {target.Url}, no response",
                                packages);
                        }

                        if (!httpResponseMessage.IsSuccessStatusCode)
                        {
                            return new AppVersion(target,
                                message ??
                                $"Could not get application metadata from target {target.Url}, status code not successful {httpResponseMessage.StatusCode}",
                                packages);
                        }

                        appMetadata =
                            await GetAppVersionAsync(httpResponseMessage, target, packages, linkedTokenSource.Token);
                    }
                }
            }

            return appMetadata;
        }

        public async Task<IReadOnlyCollection<AppVersion>> GetAppMetadataAsync(
            IReadOnlyCollection<DeploymentTarget> targets,
            CancellationToken cancellationToken)
        {
            var appVersions = new List<AppVersion>();

            using (var cancellationTokenSource =
                _timeoutHelper.CreateCancellationTokenSource(TimeSpan.FromSeconds(_monitorConfiguration.DefaultTimeoutInSeconds)))
            {
                using (var linkedTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token))
                {
                    var tasks = new Dictionary<string, Task<(HttpResponseMessage, string)>>();

                    foreach (var deploymentTarget in targets)
                    {
                        if (!deploymentTarget.Enabled)
                        {
                            appVersions.Add(new AppVersion(deploymentTarget,
                                "Disabled",
                                ImmutableArray<PackageVersion>.Empty));
                        }
                        else if (deploymentTarget.Url != null)
                        {
                            var getApplicationMetadataTask =
                                GetApplicationMetadataTask(deploymentTarget, linkedTokenSource.Token);

                            tasks.Add(deploymentTarget.Id, getApplicationMetadataTask);
                        }
                        else
                        {
                            appVersions.Add(new AppVersion(deploymentTarget,
                                "Missing URL",
                                ImmutableArray<PackageVersion>.Empty));
                        }
                    }

                    await Task.WhenAll(tasks.Values);

                    foreach (var pair in tasks)
                    {
                        var target = targets.Single(deploymentTarget => deploymentTarget.Id == pair.Key);

                        var allowedPackages =
                            await GetAllowedPackagesAsync(target, linkedTokenSource.Token);

                        try
                        {
                            (HttpResponseMessage Response, string Message) result = await pair.Value;

                            AppVersion appVersion;
                            using (var response = result.Response)
                            {
                                string message = result.Message;

                                if (response.HasValue() && response.IsSuccessStatusCode)
                                {
                                    appVersion = await GetAppVersionAsync(response,
                                        target,
                                        allowedPackages,
                                        linkedTokenSource.Token);
                                }
                                else
                                {
                                    if (message.HasValue())
                                    {
                                        var packages =
                                            await GetAllowedPackagesAsync(target, linkedTokenSource.Token);
                                        appVersion = new AppVersion(target, message, packages);

                                        _logger.Error(
                                            "Could not get metadata for target {Name} ({Url}), http status code {StatusCode}",
                                            target.Name,
                                            target.Url,
                                            response?.StatusCode.ToString() ?? Constants.NotAvailable);
                                    }
                                    else
                                    {
                                        var packages =
                                            await GetAllowedPackagesAsync(target, linkedTokenSource.Token);
                                        appVersion = new AppVersion(target, "Unknown error", packages);

                                        _logger.Error(
                                            "Could not get metadata for target {Name} ({Url}), http status code {StatusCode}",
                                            target.Name,
                                            target.Url,
                                            response?.StatusCode.ToString() ?? Constants.NotAvailable);
                                    }
                                }
                            }

                            appVersions.Add(appVersion);
                        }
                        catch (JsonReaderException ex)
                        {
                            appVersions.Add(new AppVersion(target, ex.Message, ImmutableArray<PackageVersion>.Empty));

                            _logger.Error(ex,
                                "Could not get metadata for target {Name} ({Url})",
                                target.Name,
                                target.Url);
                        }
                    }
                }
            }

            return appVersions;
        }
    }
}
