using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Processing;
using NuGet.Versioning;
using Serilog;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public class PackageService
    {
        private const string AllPackagesCacheKey = "urn:packages:all";
        private readonly NuGetListConfiguration _deploymentConfiguration;
        private readonly ICustomMemoryCache _memoryCache;
        [NotNull]
        private readonly IKeyValueConfiguration _keyValueConfiguration;

        private ILogger _logger;

        public PackageService(
            [NotNull] NuGetListConfiguration deploymentConfiguration,
            [NotNull] ICustomMemoryCache memoryCache,
            [NotNull] IKeyValueConfiguration keyValueConfiguration,
            [NotNull] ILogger logger)
        {
            _deploymentConfiguration = deploymentConfiguration ?? throw new ArgumentNullException(nameof(deploymentConfiguration));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _keyValueConfiguration = keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            if (packageId.Equals("N/A"))
            {
                return ImmutableArray<PackageVersion>.Empty;
            }

            string cacheKey = AllPackagesCacheKey;

            if (nugetConfigFile.HasValue())
            {
                string configCachePart =
                    "urn:packages:" + nugetConfigFile.Replace(Path.DirectorySeparatorChar.ToString(), "");

                if (nugetPackageSource.HasValue())
                {
                    cacheKey = configCachePart
                               + ":" + nugetPackageSource
                                   .Replace(":", "")
                                   .Replace("/", "")
                                   .Replace(".", "");
                }
                else
                {
                    cacheKey = configCachePart;
                }
            }

            cacheKey += ":" + packageId;

            if (useCache)
            {
                if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyCollection<PackageVersion> packages))
                {
                    if (packages.Count > 0)
                    {
                        return packages;
                    }
                }
            }

            string nugetExe = _keyValueConfiguration[ConfigurationConstants.NuGetExePath];

            if (string.IsNullOrWhiteSpace(nugetExe))
            {
                throw new InvalidOperationException("The nuget.exe path is not set");
            }

            if (!File.Exists(nugetExe))
            {
                throw new InvalidOperationException($"The nuget.exe path '{nugetExe}' does not exist");
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
                logger?.Debug("Using package source '{PackageSource}'", packageSource);
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

            string configFile =
                nugetConfigFile.WithDefault(_keyValueConfiguration[ConfigurationConstants.NugetConfigFile]);

            if (configFile.HasValue() && File.Exists(configFile))
            {
                args.Add("-ConfigFile");
                args.Add(configFile);
            }

            var builder = new List<string>();
            var errorBuild = new List<string>();

            logger?.Debug("Running NuGet to find packages with timeout {Seconds} seconds",
                _deploymentConfiguration.ListTimeOutInSeconds);

            ExitCode exitCode;

            using (var cancellationTokenSource =
                new CancellationTokenSource(TimeSpan.FromSeconds(_deploymentConfiguration.ListTimeOutInSeconds)))
            {
                using (CancellationTokenSource linked =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token))
                {
                    exitCode = await ProcessRunner.ExecuteAsync(nugetExe,
                        arguments: args,
                        standardOutLog: (s, _) => builder.Add(s),
                        standardErrorAction: (s, _) => errorBuild.Add(s),
                        toolAction: (s, _) => _logger.Information("{ProcessToolMessage}", s),
                        verboseAction: (s, _) => _logger.Verbose("{ProcessToolMessage}", s),
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

                await ProcessRunner.ExecuteAsync(nugetExe,
                    arguments: sourcesArgs,
                    standardOutLog: (s, _) => sources.Add(s),
                    standardErrorAction: (s, _) => sourcesError.Add(s),
                    toolAction: (s, _) => _logger.Information("{ProcessToolMessage}", s),
                    verboseAction: (s, _) => _logger.Verbose("{ProcessToolMessage}", s),
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
                                return null;
                            }

                            if (!packageId.Equals(currentPackageId, StringComparison.OrdinalIgnoreCase))
                            {
                                return null;
                            }

                            return new PackageVersion(packageId, semanticVersion);
                        }
                        catch (Exception ex) when (!ex.IsFatal())
                        {
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
                _logger.Verbose("Added {Count} packages to in-memory cache {PackageVersions}",
                    addedPackages.Count,
                    addedPackages);
            }
            else
            {
                _logger.Information("Added {Count} packages to in-memory cache",
                    addedPackages.Count);
            }

            _memoryCache.Set(cacheKey, addedPackages);

            return items;
        }
    }
}