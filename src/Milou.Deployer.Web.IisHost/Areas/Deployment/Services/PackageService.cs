﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.Tooler;
using JetBrains.Annotations;
using Milou.Deployer.Core.Processes;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Http;
using NuGet.Versioning;
using Serilog;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [UsedImplicitly]
    public class PackageService
    {
        private const string AllPackagesCacheKey = PackagesCacheKeyBaseUrn + ":AnyConfig";
        private const string PackagesCacheKeyBaseUrn = "urn:milou:deployer:web:packages:";
        private readonly NuGetListConfiguration _deploymentConfiguration;
        private readonly CustomHttpClientFactory _httpClientFactory;

        [NotNull]
        private readonly IKeyValueConfiguration _keyValueConfiguration;

        private readonly ICustomMemoryCache _memoryCache;

        private ILogger _logger;

        public PackageService(
            [NotNull] NuGetListConfiguration deploymentConfiguration,
            [NotNull] ICustomMemoryCache memoryCache,
            [NotNull] IKeyValueConfiguration keyValueConfiguration,
            [NotNull] ILogger logger,
            [NotNull] CustomHttpClientFactory httpClientFactory)
        {
            _deploymentConfiguration = deploymentConfiguration ??
                                       throw new ArgumentNullException(nameof(deploymentConfiguration));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
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

            if (packageId.Equals(Constants.NotAvailable))
            {
                return ImmutableArray<PackageVersion>.Empty;
            }

            string NormalizeKey(string key)
            {
                return key.Replace(":", "_")
                    .Replace("/", "")
                    .Replace(".", "")
                    .Replace(Path.DirectorySeparatorChar.ToString(), "_");
            }

            string cacheKey = AllPackagesCacheKey;

            if (!string.IsNullOrWhiteSpace(nugetConfigFile))
            {
                string configCachePart = $"{PackagesCacheKeyBaseUrn}:{NormalizeKey(nugetConfigFile)}";

                cacheKey = !string.IsNullOrWhiteSpace(nugetPackageSource) ? $"{configCachePart}:{NormalizeKey(nugetPackageSource)}" : configCachePart;
            }
            else if (!string.IsNullOrWhiteSpace(nugetPackageSource))
            {
                cacheKey = $"{PackagesCacheKeyBaseUrn}:{NormalizeKey(nugetPackageSource)}";
            }

            cacheKey += $":{packageId}";

            _logger.Verbose("Using package cache key {Key}", cacheKey);

            if (useCache)
            {
                if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyCollection<PackageVersion> packages))
                {
                    if (packages.Count > 0)
                    {
                        _logger.Debug("Returning packages from cache with key {Key} for package id {PackageId}",
                            cacheKey,
                            packageId);
                        return packages;
                    }
                }
            }

            var nuGetDownloadClient = new NuGetDownloadClient();
            HttpClient httpClient = _httpClientFactory.CreateClient("nuget");
            NuGetDownloadResult nuGetDownloadResult;

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                nuGetDownloadResult = await nuGetDownloadClient.DownloadNuGetAsync(NuGetDownloadSettings.Default,
                    _logger,
                    httpClient,
                    cts.Token);
            }

            if (!nuGetDownloadResult.Succeeded)
            {
                if (nuGetDownloadResult.Exception != null)
                {
                    _logger.Error(nuGetDownloadResult.Exception,
                        "Could not download NuGet.exe: {Result}",
                        nuGetDownloadResult.Result);
                }
                else
                {
                    _logger.Error("Could not download NuGet.exe: {Result}", nuGetDownloadResult.Result);
                }
            }

            string nugetExe = nuGetDownloadResult.NuGetExePath;

            if (string.IsNullOrWhiteSpace(nugetExe))
            {
                throw new DeployerAppException("The nuget.exe path is not set");
            }

            if (!File.Exists(nugetExe))
            {
                throw new DeployerAppException($"The nuget.exe path '{nugetExe}' does not exist");
            }

            string packageSourceAppSettingsKey = ConfigurationConstants.NuGetPackageSourceName;

            string packageSource = nugetPackageSource.WithDefault(_keyValueConfiguration[packageSourceAppSettingsKey]);

            var args = new List<string> { "list", packageId };

            if (includePreReleased)
            {
                args.Add("-PreRelease");
            }

            if (!string.IsNullOrWhiteSpace(packageSource))
            {
                logger?.Debug("Using package source '{PackageSource}' for package {Package}", packageSource, packageId);
                args.Add("-source");
                args.Add(packageSource);
            }
            else
            {
                logger?.Debug(
                    "There is no package source defined i app settings, key '{PackageSourceAppSettingsKey}', using all sources",
                    packageSourceAppSettingsKey);
            }

            args.Add("-AllVersions");
            args.Add("-NonInteractive");
            args.Add("-Verbosity");
            args.Add("normal");

            string configFile =
                nugetConfigFile.WithDefault(_keyValueConfiguration[ConfigurationConstants.NugetConfigFile]);

            if (configFile.HasValue() && File.Exists(configFile))
            {
                _logger.Debug("Using NuGet config file {NuGetConfigFile} for package {Package}", configFile, packageId);
                args.Add("-ConfigFile");
                args.Add(configFile);
            }

            var builder = new List<string>();
            var errorBuild = new List<string>();

            logger?.Debug("Running NuGet from package service to find packages with timeout {Seconds} seconds",
                _deploymentConfiguration.ListTimeOutInSeconds);

            ExitCode exitCode;

            using (var cancellationTokenSource =
                new CancellationTokenSource(TimeSpan.FromSeconds(_deploymentConfiguration.ListTimeOutInSeconds)))
            {
                using (CancellationTokenSource linked =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token))
                {
                    exitCode = await ProcessRunner.ExecuteProcessAsync(nugetExe,
                        args,
                        (message, category) =>
                        {
                            builder.Add(message);
                            _logger.Debug("{Category} {Message}", category, message);
                        },
                        (message, category) =>
                        {
                            errorBuild.Add(message);
                            _logger.Error("{Category} {Message}", category, message);
                        },
                        (message, category) => _logger.Debug("{Category} {ProcessToolMessage}",category, message),
                        (message, category) => _logger.Verbose("{Category} {ProcessToolMessage}",category, message),
                        cancellationToken: linked.Token);
                }
            }

            string standardOut = string.Join(Environment.NewLine, builder);
            string standardErrorOut = string.Join(Environment.NewLine, errorBuild);

            if (!exitCode.IsSuccess)
            {
                var sources = new List<string>();
                var sourcesError = new List<string>();

                var sourcesArgs = new List<string> { "sources" };

                if (configFile.HasValue() && File.Exists(configFile))
                {
                    sourcesArgs.Add("-ConfigFile");
                    sourcesArgs.Add(configFile);
                }

                sourcesArgs.Add("-NonInteractive");

                if (_logger.IsEnabled(LogEventLevel.Debug) || _logger.IsEnabled(LogEventLevel.Verbose))
                {
                    sourcesArgs.Add("-Verbosity");
                    sourcesArgs.Add("detailed");
                }

                await ProcessRunner.ExecuteProcessAsync(nugetExe,
                    sourcesArgs,
                    (message, _) => sources.Add(message),
                    (message, _) => sourcesError.Add(message),
                    (message, category) => _logger.Information("{Category} {ProcessToolMessage}",category, message),
                    (message, category) => _logger.Verbose("{Category} {ProcessToolMessage}",category,message),
                    cancellationToken: cancellationToken);

                string sourcesOut = string.Join(Environment.NewLine, sources);
                string sourcesErrorOut = string.Join(Environment.NewLine, sourcesError);

                _logger.Error(
                    "Exit code {Code} when running NuGet list packages; standard out '{StandardOut}', standard error '{StandardErrorOut}', exe path '{NugetExe}', arguments '{Arguments}', nuget sources '{SourcesOut}', sources error '{SourcesErrorOut}'",
                    exitCode.Code,
                    standardOut,
                    standardErrorOut,
                    nugetExe,
                    string.Join(" ", args),
                    sourcesOut,
                    sourcesErrorOut);

                return Array.Empty<PackageVersion>();
            }

            var ignoredOutputStatements = new List<string> { "Using credentials", "No packages found" };

            List<string> included =
                builder.Where(line => !ignoredOutputStatements.Any(ignored =>
                        line.IndexOf(ignored, StringComparison.InvariantCultureIgnoreCase) >= 0))
                    .ToList();

            List<PackageVersion> items = included.Select(
                    package =>
                    {
                        string[] parts = package.Split(' ');

                        string currentPackageId = parts[0];

                        try
                        {
                            string version = parts.Last();

                            if (!SemanticVersion.TryParse(version, out SemanticVersion semanticVersion))
                            {
                                _logger.Debug(
                                    "Found package version {Version} for package {Package}, skipping because it could not be parsed as semantic version",
                                    version,
                                    currentPackageId);
                                return null;
                            }

                            if (!packageId.Equals(currentPackageId, StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.Debug(
                                    "Found package {Package}, skipping because it does match requested package {RequestedPackage}",
                                    currentPackageId,
                                    packageId);

                                return null;
                            }

                            return new PackageVersion(packageId, semanticVersion);
                        }
                        catch (Exception ex) when (!ex.IsFatal())
                        {
                            _logger.Warning(ex, "Error parsing package '{Package}'", package);
                            return null;
                        }
                    })
                .Where(packageVersion => packageVersion != null)
                .OrderBy(packageVersion => packageVersion.PackageId)
                .ThenByDescending(packageVersion => packageVersion.Version)
                .ToList();

            var addedPackages = new List<string>();

            foreach (PackageVersion packageVersion in items)
            {
                addedPackages.Add(packageVersion.ToString());
            }

            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
                _logger.Verbose("Added {Count} packages to in-memory cache with cache key {CacheKey} {PackageVersions}",
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
                _logger.Information("Added {Count} packages to in-memory cache with cache key {CacheKey}",
                    addedPackages.Count,
                    cacheKey);
            }

            if (addedPackages.Any())
            {
                _memoryCache.Set(cacheKey, items);
            }
            else
            {
                _logger.Debug("Added no packages to in-memory cache for cache key {CacheKey}", cacheKey);
            }

            return items;
        }
    }
}