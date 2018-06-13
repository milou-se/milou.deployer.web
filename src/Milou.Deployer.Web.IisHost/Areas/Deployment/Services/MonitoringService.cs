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
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Newtonsoft.Json;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [UsedImplicitly]
    public class MonitoringService
    {
        private readonly DeploymentService _deploymentService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITime _time;
        private readonly ILogger _logger;
        private readonly PackageService _packageService;

        public MonitoringService(
            [NotNull] ILogger logger,
            [NotNull] DeploymentService deploymentService,
            [NotNull] IHttpClientFactory httpClientFactory,
            [NotNull] ITime time,
            [NotNull] PackageService packageService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
        }

        public async Task<AppVersion> GetAppMetadataAsync(
            [NotNull] DeploymentTarget target,
            CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            cancellationTokenSource.Token.Register(() => { Debug.WriteLine("GetAppMetadataAsync token timed out"); });

            CancellationTokenSource linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);

            (HttpResponseMessage Response, string Message) result =
                await GetApplicationMetadataTask(target, linkedTokenSource.Token);

            HttpResponseMessage httpResponseMessage = result.Response;

            IReadOnlyCollection<PackageVersion> packages = await GetAllowedPackagesAsync(target);

            if (httpResponseMessage == null)
            {
                return new AppVersion(target,
                    result.Message ?? $"Could not get application metadata from target {target.Url}", packages);
            }

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return new AppVersion(target,
                    result.Message ?? $"Could not get application metadata from target {target.Url}", packages);
            }

            AppVersion appMetadata =
                await GetAppVersionAsync(httpResponseMessage, target, packages);

            return appMetadata;
        }

        public async Task<IReadOnlyCollection<AppVersion>> GetAppMetadataAsync(
            IReadOnlyCollection<DeploymentTarget> targets,
            CancellationToken cancellationToken = default)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            CancellationTokenSource linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);

            var appVersions = new List<AppVersion>();

                var tasks = new Dictionary<string, Task<(HttpResponseMessage, string)>>();

                foreach (DeploymentTarget deploymentTarget in targets)
                {
                    if (deploymentTarget.Url != null)
                    {
                        Task<(HttpResponseMessage, string)> getApplicationMetadataTask =
                            GetApplicationMetadataTask(deploymentTarget, linkedTokenSource.Token);

                        tasks.Add(deploymentTarget.Id, getApplicationMetadataTask);
                    }
                    else
                    {
                        appVersions.Add(new AppVersion(deploymentTarget, "Missing URL", ImmutableArray<PackageVersion>.Empty));
                    }
                }

                await Task.WhenAll(tasks.Values);

                foreach (KeyValuePair<string, Task<(HttpResponseMessage, string)>> pair in tasks)
                {
                    DeploymentTarget target = targets.Single(deploymentTarget => deploymentTarget.Id == pair.Key);

                    IReadOnlyCollection<PackageVersion> allowedPackages = await GetAllowedPackagesAsync(target);

                    try
                    {
                        (HttpResponseMessage Response, string Message) result = await pair.Value;

                        HttpResponseMessage response = result.Response;
                        string message = result.Message;

                        AppVersion appVersion;

                        if (response.HasValue() && response.IsSuccessStatusCode)
                        {
                            appVersion = await GetAppVersionAsync(response, target, allowedPackages);
                        }
                        else
                        {
                            if (message.HasValue())
                            {
                                IReadOnlyCollection<PackageVersion> packages = await GetAllowedPackagesAsync(target);
                                appVersion = new AppVersion(target, message, packages);

                                _logger.Error("Could not get metadata for target {Name} ({Url}), http status code {V}",
                                    target.Name,
                                    target.Url,
                                    response?.StatusCode.ToString() ?? "N/A");
                            }
                            else
                        {
                            IReadOnlyCollection<PackageVersion> packages = await GetAllowedPackagesAsync(target);
                            appVersion = new AppVersion(target, "Unknown error", packages);

                                _logger.Error("Could not get metadata for target {Name} ({Url}), http status code {V}",
                                    target.Name,
                                    target.Url,
                                    response?.StatusCode.ToString() ?? "N/A");
                            }
                        }

                        appVersions.Add(appVersion);
                    }
                    catch (JsonReaderException ex)
                    {
                        appVersions.Add(new AppVersion(target, ex.Message, ImmutableArray<PackageVersion>.Empty));

                        _logger.Error(ex, "Could not get metadata for target {Name} ({Url})", target.Name, target.Url);
                    }
                }


            return appVersions;
        }

        private async Task<AppVersion> GetAppVersionAsync(
            HttpResponseMessage response,
            DeploymentTarget target,
            IReadOnlyCollection<PackageVersion> filtered)
        {
            if (response.Content.Headers.ContentType is null ||
                !response.Content.Headers.ContentType.MediaType.Equals("application/json",
                    StringComparison.OrdinalIgnoreCase))
            {
                return new AppVersion(target, "Response not JSON", filtered);
            }

            string json = await response.Content.ReadAsStringAsync();
            ConfigurationItems configuration =
                new JsonConfigurationSerializer().Deserialize(json);

            var nameValueCollection = new NameValueCollection();

            foreach (KeyValue configurationItem in configuration.Keys)
            {
                nameValueCollection.Add(configurationItem.Key, configurationItem.Value);
            }

            var appVersion = new AppVersion(target, new InMemoryKeyValueConfiguration(nameValueCollection), filtered, _time.UtcNow());

            return appVersion;
        }

        private async Task<IReadOnlyCollection<PackageVersion>> GetAllowedPackagesAsync(DeploymentTarget target)
        {
            IReadOnlyCollection<PackageVersion> allPackageVersions =
                await _packageService.GetPackageVersionsAsync(target.PackageId, nugetConfigFile: target.NuGetConfigFile,
                    nugetPackageSource: target.NuGetPackageSource, logger: _logger);

            IReadOnlyCollection<PackageVersion> allTargetPackageVersions = allPackageVersions.Where(
                    packageVersion =>
                        target.PackageId.Equals(packageVersion.PackageId,
                            StringComparison.OrdinalIgnoreCase))
                .SafeToReadOnlyCollection();

            IReadOnlyCollection<PackageVersion> preReleaseFiltered = allTargetPackageVersions;

            if (!target.AllowPrerelease)
            {
                preReleaseFiltered =
                    allTargetPackageVersions.Where(package => !package.Version.IsPrerelease).SafeToReadOnlyCollection();
            }

            IReadOnlyCollection<PackageVersion> filtered =
                preReleaseFiltered.OrderByDescending(packageVersion => packageVersion.Version)
                    .SafeToReadOnlyCollection();
            return filtered;
        }

        private Task<(HttpResponseMessage, string)> GetApplicationMetadataTask(
            DeploymentTarget deploymentTarget,
            CancellationToken cancellationToken)
        {
            var uriBuilder = new UriBuilder(deploymentTarget.Url)
            {
                Path = "applicationmetadata.json"
            };

            Uri applicationMetadataUri = uriBuilder.Uri;

            Task<(HttpResponseMessage, string)> getApplicationMetadataTask = GetWrappedResponseAsync(

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
                HttpClient client = _httpClientFactory.CreateClient(applicationMetadataUri.Host);
                return (await client.GetAsync(applicationMetadataUri, cancellationToken), "");
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
                _logger.Error(ex, "Could not get application metadata for {ApplicationMetadataUri}",
                    applicationMetadataUri);
                return ((HttpResponseMessage)null, ex.Message);
            }
        }
    }
}