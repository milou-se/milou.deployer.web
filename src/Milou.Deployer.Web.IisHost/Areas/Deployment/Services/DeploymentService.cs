using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Email;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Processing;
using Milou.Deployer.Web.IisHost.Areas.Logging;
using NuGet.Versioning;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [UsedImplicitly]
    public class DeploymentService
    {
        private const string AllPackagesCacheKey = "urn:packages:all";
        private readonly MilouDeployer _deployer;
        private readonly IKeyValueConfiguration _keyValueConfiguration;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly ICustomMemoryCache _memoryCache;

        private readonly IDeploymentTargetReadService _targetSource;
        private readonly NuGetListConfiguration _deploymentConfiguration;
        private readonly ITime _time;

        public DeploymentService(
            [NotNull] ILogger logger,
            [NotNull] IDeploymentTargetReadService targetSource,
            [NotNull] IMediator mediator,
            [NotNull] MilouDeployer deployer,
            [NotNull] IKeyValueConfiguration keyValueConfiguration,
            [NotNull] ICustomMemoryCache memoryCache,
            NuGetListConfiguration deploymentConfiguration, ITime time)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _targetSource = targetSource ?? throw new ArgumentNullException(nameof(targetSource));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _deployer = deployer ?? throw new ArgumentNullException(nameof(deployer));
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _deploymentConfiguration = deploymentConfiguration;
            _time = time;
        }

        public async Task<DeploymentTaskResult> ExecuteDeploymentAsync(
            [NotNull] DeploymentTask deploymentTask,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (deploymentTask == null)
            {
                throw new ArgumentNullException(nameof(deploymentTask));
            }

            DateTime start = _time.UtcNow().DateTime;
            Stopwatch stopwatch = Stopwatch.StartNew();

            (ExitCode, DateTime) result;

            DirectoryInfo deploymentJobsDirectory = EnsureDeploymentJobsDirectoryExists();

            DeploymentTarget deploymentTarget = null;

            try
            {
                deploymentTarget = await GetDeploymentTargetAsync(deploymentTask,
                    cancellationToken);

                VerifyPreReleaseAllowed(deploymentTask.SemanticVersion,
                    deploymentTarget,
                    deploymentTask.PackageId,
                    logger);

                VerifyAllowedPackageIsAllowed(deploymentTarget, deploymentTask.PackageId, logger);

                result = await RunDeploymentToolAsync(deploymentTask,
                    deploymentJobsDirectory,
                    deploymentTarget,
                    logger);
            }
            catch (Exception ex)
            {
                result = (ExitCode.Failure, _time.UtcNow().DateTime);
                logger.Error(ex, "Error deploying");
            }

            stopwatch.Stop();

            string metadataContent = LogJobMetadata(deploymentTask,
                start,
                result.Item2,
                stopwatch,
                result.Item1,
                deploymentJobsDirectory,
                deploymentTarget);

            var deploymentTaskResult = new DeploymentTaskResult(deploymentTask.DeploymentTaskId, deploymentTask.DeploymentTargetId, result.Item1, start, result.Item2, metadataContent);

            await _mediator.Publish(new DeploymentMetadataLogNotification(deploymentTask, deploymentTaskResult), cancellationToken);

            return deploymentTaskResult;
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

            string configFile = nugetConfigFile.WithDefault(_keyValueConfiguration[ConfigurationConstants.NugetConfigFile]);

            if (configFile.HasValue() && File.Exists(configFile))
            {
                args.Add("-ConfigFile");
                args.Add(configFile);
            }

            var builder = new List<string>();
            var errorBuild = new List<string>();

            logger?.Debug("Running NuGet to find packages with timeout {Seconds} seconds", _deploymentConfiguration.ListTimeOutInSeconds);

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

                _logger.Error("Exit code {Code} when running NuGet list packages; standard out '{StandardOut}', standard error '{StandardErrorOut}', exe path '{NugetExe}', arguments '{Arguments}', nuget sources '{SourcesOut}', sources error '{SourcesErrorOut}'", exitCode.Code, standardOut, standardErrorOut, nugetExe, string.Join(" ", args), sourcesOut, sourcesErrorOut);
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

        private string LogJobMetadata(
            DeploymentTask deploymentTask,
            DateTime start,
            DateTime end,
            Stopwatch stopwatch,
            ExitCode exitCode,
            DirectoryInfo deploymentJobsDirectory,
            DeploymentTarget deploymentTarget)
        {
            var metadata = new StringBuilder();

            metadata.AppendLine(
                $"Started job {deploymentTask.DeploymentTaskId} at {start:O} and finished at {end:O}");

            metadata.AppendLine($"Total time {stopwatch.Elapsed.TotalSeconds:f} seconds");
            metadata.AppendLine($"Package version: {deploymentTask.SemanticVersion}");
            metadata.AppendLine($"Package id: {deploymentTask.PackageId}");
            metadata.AppendLine($"Target id: {deploymentTask.DeploymentTargetId}");

            if (deploymentTarget is null)
            {
                metadata.AppendLine("Deployment target not found");
            }
            else
            {
                metadata.AppendLine($"Publish settings file: {deploymentTarget.PublishSettingFile}");
                metadata.AppendLine($"Target directory: {deploymentTarget.TargetDirectory}");
                metadata.AppendLine($"Target URI: {deploymentTarget.Url}");
            }

            metadata.AppendLine($"Exit code {exitCode}");

            string metadataContent = metadata.ToString();

            string metadataFilePath = Path.Combine(deploymentJobsDirectory.FullName,
                $"{deploymentTask.DeploymentTaskId}.metadata.txt");

            File.WriteAllText(metadataFilePath, metadataContent, Encoding.UTF8);

            return metadataContent;
        }

        private static string GetMainLogFilePath(DeploymentTask deploymentTask, DirectoryInfo deploymentJobsDirectory)
        {
            string contentFilePath = Path.Combine(deploymentJobsDirectory.FullName,
                $"{deploymentTask.DeploymentTaskId}.txt");
            return contentFilePath;
        }

        private static void CheckPackageMatchingTarget(DeploymentTarget deploymentTarget, string packageId)
        {
            if (
                !deploymentTarget.PackageId.Equals(packageId,
                        StringComparison.InvariantCultureIgnoreCase))
            {
                string allPackageIds = string.Join(", ",
                    deploymentTarget.PackageId.Select(name => $"'{name}'"));

                throw new InvalidOperationException(
                    $"The package id '{packageId}' is not in the list of allowed package ids: {allPackageIds}");
            }
        }

        private DirectoryInfo EnsureDeploymentJobsDirectoryExists()
        {
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "App_Data");

            string baseDir = _keyValueConfiguration["urn:milou:deployer:jobs-directory"]
                .WithDefault(directoryPath);

            string jobDirectoryPath = Path.Combine(baseDir, "DeploymentJobs");

            var directoryInfo = new DirectoryInfo(jobDirectoryPath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            return directoryInfo;
        }

        private async Task<(ExitCode, DateTime)> RunDeploymentToolAsync(
            DeploymentTask deploymentTask,
            DirectoryInfo deploymentJobsDirectory,
            DeploymentTarget deploymentTarget,
            ILogger logger)
        {
            string contentFilePath = GetMainLogFilePath(deploymentTask,
                deploymentJobsDirectory);

            ExitCode exitCode;

            var logBuilder = new StringBuilder();

            using (Logger log = new LoggerConfiguration()
                .WriteTo.File(contentFilePath)
                .WriteTo.DelegateSink(deploymentTask.Log)
                .WriteTo.DelegateSink(message => logBuilder.AppendLine(message))
                .CreateLogger())
            {
                logger.Debug(
                    "Running tool '{Deployer}' for deployment target '{DeploymentTarget}', package '{PackageId}' version {Version}",
                    _deployer,
                    deploymentTarget,
                    deploymentTask.PackageId,
                    deploymentTask.SemanticVersion.ToNormalizedString());

                try
                {
                    exitCode = await _deployer.ExecuteAsync(deploymentTask, log);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to deploy");
                    exitCode = ExitCode.Failure;
                }
            }

            DateTime finishedAtUtc = _time.UtcNow().DateTime;

            await _mediator.Publish(new DeploymentFinishedNotification(deploymentTask, logBuilder.ToString(), finishedAtUtc));

            return (exitCode, finishedAtUtc);
        }

        private void VerifyPreReleaseAllowed(
            SemanticVersion version,
            DeploymentTarget deploymentTarget,
            string packageId,
            ILogger logger)
        {
            if (version.IsPrerelease && !deploymentTarget.AllowPrerelease)
            {
                throw new InvalidOperationException(
                    $"Could not deploy package with id '{packageId}' to target '{deploymentTarget}' because the package is a pre-release version and the target does not support it");
            }

            if (version.IsPrerelease)
            {
                logger.Debug(
                    "The deployment target '{DeploymentTarget}' allows package id '{PackageId}' version {Version}, pre-release",
                    deploymentTarget,
                    packageId,
                    version.ToNormalizedString());
            }
        }

        private void VerifyAllowedPackageIsAllowed(DeploymentTarget deploymentTarget, string packageId, ILogger logger)
        {
            if (deploymentTarget.PackageId.Any())
            {
                CheckPackageMatchingTarget(deploymentTarget, packageId);

                logger.Debug("The deployment target '{DeploymentTarget}' allows package id '{PackageId}'",
                    deploymentTarget,
                    packageId);
            }
            else
            {
                logger.Debug(
                    "The deployment target '{DeploymentTarget}' has no allowed package names, allowing any package id",
                    deploymentTarget);
            }
        }

        private async Task<DeploymentTarget> GetDeploymentTargetAsync(
            DeploymentTask deploymentTask,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<DeploymentTarget> targets =
                (await _targetSource.GetOrganizationsAsync(cancellationToken))
                .SelectMany(
                    organization => organization.Projects.SelectMany(project => project.DeploymentTargets))
                .SafeToReadOnlyCollection();

            DeploymentTarget deploymentTarget =
                targets.SingleOrDefault(target => target.Id.Equals(deploymentTask.DeploymentTargetId));

            if (deploymentTarget == null)
            {
                throw new InvalidOperationException(
                    $"Deployment target with id '{deploymentTask.DeploymentTargetId}' was not found using source {_targetSource.GetType().FullName}");
            }

            return deploymentTarget;
        }
    }
}