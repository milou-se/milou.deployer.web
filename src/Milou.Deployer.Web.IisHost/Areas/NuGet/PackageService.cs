using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Core;
using Arbor.Tooler;

using JetBrains.Annotations;

using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Caching;
using Milou.Deployer.Web.Core.Deployment.Packages;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.NuGet;
using Milou.Deployer.Web.Core.Settings;

using Serilog;
using Serilog.Events;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Milou.Deployer.Web.IisHost.Areas.NuGet
{
    [UsedImplicitly]
    public class PackageService
    {
        private const string AllPackagesCacheKey = PackagesCacheKeyBaseUrn + ":AnyConfig";

        private const string PackagesCacheKeyBaseUrn = "urn:milou:deployer:web:packages:";

        private readonly IApplicationSettingsStore _applicationSettingsStore;

        private readonly NuGetListConfiguration _deploymentConfiguration;

        [NotNull]
        private readonly IKeyValueConfiguration _keyValueConfiguration;

        private readonly ILogger _logger;

        private readonly ICustomMemoryCache _memoryCache;

        private readonly NuGetConfiguration _nuGetConfiguration;

        private readonly NuGetPackageInstaller _packageInstaller;

        public PackageService(
            [NotNull] NuGetListConfiguration deploymentConfiguration,
            [NotNull] ICustomMemoryCache memoryCache,
            [NotNull] IKeyValueConfiguration keyValueConfiguration,
            [NotNull] ILogger logger,
            [NotNull] NuGetConfiguration nuGetConfiguration,
            [NotNull] NuGetPackageInstaller packageInstaller,
            IApplicationSettingsStore applicationSettingsStore)
        {
            _deploymentConfiguration = deploymentConfiguration ??
                                       throw new ArgumentNullException(nameof(deploymentConfiguration));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nuGetConfiguration = nuGetConfiguration ?? throw new ArgumentNullException(nameof(nuGetConfiguration));
            _packageInstaller = packageInstaller ?? throw new ArgumentNullException(nameof(packageInstaller));
            _applicationSettingsStore = applicationSettingsStore;
        }

        public void ClearCache(string packageId, string notificationNugetConfig, string notificationNugetSource)
        {
            string cacheKey = GetCacheKey(notificationNugetConfig, notificationNugetSource, packageId);

            _memoryCache.Invalidate(cacheKey);
        }

        public async Task<IReadOnlyCollection<PackageVersion>> GetPackageVersionsAsync(
            [NotNull] string packageId,
            bool useCache = true,
            ILogger logger = null,
            bool includePreReleased = false,
            string nugetPackageSource = null,
            string nugetConfigFile = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageId));
            }

            if (packageId.Equals(Constants.NotAvailable, StringComparison.OrdinalIgnoreCase))
            {
                return ImmutableArray<PackageVersion>.Empty;
            }

            string cacheKey = GetCacheKey(nugetConfigFile, nugetPackageSource, packageId);

            _logger.Verbose("Using package cache key {Key}", cacheKey);

            if (useCache && _memoryCache.TryGetValue(cacheKey, out IReadOnlyCollection<PackageVersion> packages)
                         && packages.Count > 0)
            {
                _logger.Debug(
                    "Returning packages from cache with key {Key} for package id {PackageId}",
                    cacheKey,
                    packageId);
                return packages;
            }

            if (string.IsNullOrWhiteSpace(_nuGetConfiguration.NugetExePath))
            {
                throw new DeployerAppException("The nuget.exe path is not set");
            }

            if (!File.Exists(_nuGetConfiguration.NugetExePath))
            {
                throw new DeployerAppException(
                    $"The nuget.exe path '{_nuGetConfiguration.NugetExePath}' does not exist");
            }

            string packageSourceAppSettingsKey = ConfigurationConstants.NuGetPackageSourceName;

            string packageSource = nugetPackageSource.WithDefault(_keyValueConfiguration[packageSourceAppSettingsKey]);

            if (!string.IsNullOrWhiteSpace(packageSource))
            {
                logger?.Debug("Using package source '{PackageSource}' for package {Package}", packageSource, packageId);
            }
            else
            {
                logger?.Debug(
                    "There is no package source defined in app settings, key '{PackageSourceAppSettingsKey}', using all sources",
                    packageSourceAppSettingsKey);
            }

            string configFile =
                nugetConfigFile.WithDefault(_keyValueConfiguration[ConfigurationConstants.NugetConfigFile]);

            if (configFile.HasValue() && File.Exists(configFile))
            {
                _logger.Debug("Using NuGet config file {NuGetConfigFile} for package {Package}", configFile, packageId);
            }

            logger?.Debug(
                "Running NuGet from package service to find package {PackageId} with timeout {Seconds} seconds",
                packageId,
                _deploymentConfiguration.ListTimeOutInSeconds);

            Stopwatch stopwatch = Stopwatch.StartNew();

            var allVersions = await _packageInstaller.GetAllVersionsAsync(
                                  new NuGetPackageId(packageId),
                                  nuGetSource: nugetPackageSource,
                                  nugetConfig: nugetConfigFile,
                                  allowPreRelease: includePreReleased,
                                  nugetExePath: _nuGetConfiguration.NugetExePath);

            stopwatch.Stop();

            _logger.Debug(
                "Get package versions external process took {Elapsed} milliseconds",
                stopwatch.ElapsedMilliseconds);

            var addedPackages = new List<string>();

            IReadOnlyCollection<PackageVersion> packageVersions = allVersions
                .Select(version => new PackageVersion(packageId, version))
                .ToArray();

            foreach (var packageVersion in packageVersions)
            {
                _logger.Debug(
                    "Found package {Package} {Version}",
                    packageVersion.PackageId,
                    packageVersion.Version.ToNormalizedString());

                addedPackages.Add(packageVersion.ToString());
            }

            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
                _logger.Verbose(
                    "Added {Count} packages to in-memory cache with cache key {CacheKey} {PackageVersions}",
                    addedPackages.Count,
                    cacheKey,
                    addedPackages);
            }
            else if (addedPackages.Count > 0 && addedPackages.Count < 20)
            {
                _logger.Information(
                    "Added {Count} packages to in-memory cache with cache key {CacheKey} {PackageVersions}",
                    addedPackages.Count,
                    cacheKey,
                    addedPackages);
            }
            else if (addedPackages.Any())
            {
                _logger.Information(
                    "Added {Count} packages to in-memory cache with cache key {CacheKey}",
                    addedPackages.Count,
                    cacheKey);
            }

            if (addedPackages.Any())
            {
                var settings = await _applicationSettingsStore.GetApplicationSettings(CancellationToken.None);
                TimeSpan cacheTime = settings.CacheTime;
                _memoryCache.SetValue(cacheKey, packageVersions, cacheTime);
                _logger.Debug(
                    "Cached {Packages} packages with key {CacheKey} for {Duration} seconds",
                    addedPackages.Count,
                    cacheKey,
                    cacheTime.TotalSeconds.ToString("F0"));
            }
            else
            {
                _logger.Debug("Added no packages to in-memory cache for cache key {CacheKey}", cacheKey);
            }

            return packageVersions;
        }

        private string GetCacheKey(string nugetConfigFile, string nugetPackageSource, string packageId)
        {
            string cacheKey = AllPackagesCacheKey;

            if (!string.IsNullOrWhiteSpace(nugetConfigFile))
            {
                string configCachePart = $"{PackagesCacheKeyBaseUrn}:{NormalizeKey(nugetConfigFile)}";

                cacheKey = !string.IsNullOrWhiteSpace(nugetPackageSource)
                               ? $"{configCachePart}:{NormalizeKey(nugetPackageSource)}"
                               : configCachePart;
            }
            else if (!string.IsNullOrWhiteSpace(nugetPackageSource))
            {
                cacheKey = $"{PackagesCacheKeyBaseUrn}:{NormalizeKey(nugetPackageSource)}";
            }

            cacheKey += $":{packageId}";

            return cacheKey;
        }

        private string NormalizeKey(string key) =>
            key.Replace(":", "_", StringComparison.OrdinalIgnoreCase)
                .Replace("/", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace(".", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace(
                    Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture),
                    "_",
                    StringComparison.OrdinalIgnoreCase);
    }
}