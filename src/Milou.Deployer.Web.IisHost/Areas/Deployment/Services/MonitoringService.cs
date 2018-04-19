using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Schema.Json;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Caching;
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
        private readonly ILogger _logger;

        public MonitoringService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<AppVersion> GetAppMetadataAsync(
            [NotNull] DeploymentTarget target,
            CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            CancellationTokenSource linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);

            using (var client = new HttpClient())
            {
                Tuple<HttpResponseMessage, string> tuple =
                    await GetApplicationMetadataTask(target, client, linkedTokenSource.Token);

                HttpResponseMessage httpResponseMessage = tuple.Item1;

                if (httpResponseMessage == null || !httpResponseMessage.IsSuccessStatusCode)
                {
                    return new AppVersion(target, tuple.Item2 ?? "Could not get application metadata");
                }

                AppVersion appMetadata =
                    await GetAppVersionAsync(httpResponseMessage, target, GetAllowedPackages(target));

                return appMetadata;
            }
        }

        public async Task<IReadOnlyCollection<AppVersion>> GetAppMetadataAsync(
            IReadOnlyCollection<DeploymentTarget> targets,
            CancellationToken cancellationToken = default)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            CancellationTokenSource linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);

            var appVersions = new List<AppVersion>();

            using (var client = new HttpClient())
            {
                var tasks = new Dictionary<string, Task<Tuple<HttpResponseMessage, string>>>();

                foreach (DeploymentTarget deploymentTarget in targets)
                {
                    if (deploymentTarget.Uri != null)
                    {
                        Task<Tuple<HttpResponseMessage, string>> getApplicationMetadataTask =
                            GetApplicationMetadataTask(deploymentTarget, client, linkedTokenSource.Token);

                        tasks.Add(deploymentTarget.Id, getApplicationMetadataTask);
                    }
                    else
                    {
                        IReadOnlyCollection<PackageVersion> allowedPackages = GetAllowedPackages(deploymentTarget);
                        appVersions.Add(new AppVersion(deploymentTarget,
                            new InMemoryKeyValueConfiguration(new NameValueCollection()),
                            allowedPackages));
                    }
                }

                await Task.WhenAll(tasks.Values);

                foreach (KeyValuePair<string, Task<Tuple<HttpResponseMessage, string>>> pair in tasks)
                {
                    DeploymentTarget target = targets.Single(deploymentTarget => deploymentTarget.Id == pair.Key);

                    IReadOnlyCollection<PackageVersion> allowedPackages = GetAllowedPackages(target);

                    try
                    {
                        Tuple<HttpResponseMessage, string> tuple = await pair.Value;

                        HttpResponseMessage response = tuple.Item1;
                        string mesage = tuple.Item2;

                        AppVersion appVersion;

                        if (response != null && response.IsSuccessStatusCode)
                        {
                            appVersion = await GetAppVersionAsync(response, target, allowedPackages);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(mesage))
                            {
                                appVersion = new AppVersion(target, mesage);

                                _logger.Error("Could not get metadata for target {Name} ({Uri}), http status code {V}", target.Name, target.Uri, response?.StatusCode.ToString() ?? "N/A");
                            }
                            else
                            {
                                appVersion = new AppVersion(target,
                                    new InMemoryKeyValueConfiguration(new NameValueCollection()),
                                    allowedPackages);

                                _logger.Error("Could not get metadata for target {Name} ({Uri}), http status code {V}", target.Name, target.Uri, response?.StatusCode.ToString() ?? "N/A");
                            }
                        }

                        appVersions.Add(appVersion);
                    }
                    catch (JsonReaderException ex)
                    {
                        appVersions.Add(new AppVersion(target,
                            new InMemoryKeyValueConfiguration(new NameValueCollection()),
                            allowedPackages));

                        _logger.Error("Could not get metadata for target {Name} ({Uri}), {Ex}", target.Name, target.Uri, ex);
                    }
                }
            }

            return appVersions;
        }

        private static IReadOnlyCollection<PackageVersion> GetAllowedPackages(DeploymentTarget target)
        {
            ImmutableArray<PackageVersion> allPackageVersions = InMemoryCache.All.ToImmutableArray();

            IReadOnlyCollection<PackageVersion> allTargetPackageVersions;

            if (target.AllowedPackageNames.Any(packageName =>
                packageName.Equals("*", StringComparison.OrdinalIgnoreCase)))
            {
                allTargetPackageVersions = allPackageVersions;
            }
            else
            {
                allTargetPackageVersions =
                    allPackageVersions.Where(
                            packageVersion =>
                                target.AllowedPackageNames.Any(
                                    allowed =>
                                        allowed.Equals(packageVersion.PackageId,
                                            StringComparison.InvariantCultureIgnoreCase)))
                        .SafeToReadOnlyCollection();
            }

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

        private static async Task<AppVersion> GetAppVersionAsync(
            HttpResponseMessage response,
            DeploymentTarget target,
            IReadOnlyCollection<PackageVersion> filtered)
        {
            string json = await response.Content.ReadAsStringAsync();
            ConfigurationItems configuration =
                new JsonConfigurationSerializer().Deserialize(json);

            var nameValueCollection = new NameValueCollection();
            foreach (KeyValue configurationItem in configuration.Keys)
            {
                nameValueCollection.Add(configurationItem.Key, configurationItem.Value);
            }

            var appVersion = new AppVersion(target, new InMemoryKeyValueConfiguration(nameValueCollection), filtered);

            return appVersion;
        }

        private Task<Tuple<HttpResponseMessage, string>> GetApplicationMetadataTask(
            DeploymentTarget deploymentTarget,
            HttpClient client,
            CancellationToken cancellationToken)
        {
            var uriBuilder = new UriBuilder(deploymentTarget.Uri)
            {
                Path = "applicationmetadata.json"
            };

            Uri applicationMetadataUri = uriBuilder.Uri;

            Task<Tuple<HttpResponseMessage, string>> getApplicationMetadataTask = GetWrappedResponseAsync(
                client,
                applicationMetadataUri,
                cancellationToken);
            return getApplicationMetadataTask;
        }

        private async Task<Tuple<HttpResponseMessage, string>> GetWrappedResponseAsync(
            HttpClient httpClient,
            Uri applicationMetadataUri,
            CancellationToken cancellationToken)
        {
            Log.Debug("Making metadata request to {RequestUri}", applicationMetadataUri);

            try
            {
                return Tuple.Create(await httpClient.GetAsync(applicationMetadataUri, cancellationToken), "");
            }
            catch (TaskCanceledException ex)
            {
                _logger.Error(ex, "Could not get application metadata for {ApplicationMetadataUri}", applicationMetadataUri);
                return Tuple.Create((HttpResponseMessage)null, "Timeout");
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Error("Could not get application metadata for {ApplicationMetadataUri}, {Ex}", applicationMetadataUri, ex);
                return Tuple.Create((HttpResponseMessage)null, ex.Message);
            }
        }
    }
}