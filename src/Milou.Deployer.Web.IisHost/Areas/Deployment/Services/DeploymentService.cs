using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core.Caching;
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
        private readonly MilouDeployer _deployer;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        private readonly IDeploymentTargetReadService _targetSource;

        public DeploymentService(
            [NotNull] ILogger logger,
            [NotNull] IDeploymentTargetReadService targetSource,
            [NotNull] IMediator mediator,
            [NotNull] MilouDeployer deployer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _targetSource = targetSource ?? throw new ArgumentNullException(nameof(targetSource));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _deployer = deployer ?? throw new ArgumentNullException(nameof(deployer));
        }

        public async Task<string> ExecuteDeploymentAsync(
            [NotNull] DeploymentTask deploymentTask,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (deploymentTask == null)
            {
                throw new ArgumentNullException(nameof(deploymentTask));
            }

            DateTime start = DateTime.UtcNow;
            Stopwatch stopwatch = Stopwatch.StartNew();

            ExitCode exitCode;

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

                exitCode = await RunDeploymentToolAsync(deploymentTask,
                    deploymentJobsDirectory,
                    deploymentTarget,
                    logger);
            }
            catch (Exception ex)
            {
                exitCode = ExitCode.Failure;
                logger.Error(ex, "Error deploying");
            }

            stopwatch.Stop();

            DateTime end = DateTime.UtcNow;

            string metadataContent = LogJobMetadata(deploymentTask,
                start,
                end,
                stopwatch,
                exitCode,
                deploymentJobsDirectory,
                deploymentTarget);

            await _mediator.Publish(
                new DeploymentFinishedNotification(deploymentTask, metadataContent),
                cancellationToken);

            return metadataContent;
        }

        public async Task<IReadOnlyCollection<PackageVersion>> GetPackageVersionsAsync(
            string prefix = null,
            bool useCache = true,
            ILogger logger = null)
        {
            if (InMemoryCache.All.Any() && useCache)
            {
                return InMemoryCache.All;
            }

            string nugetExe = StaticKeyValueConfigurationManager.AppSettings[ConfigurationConstants.NuGetExePath];

            if (string.IsNullOrWhiteSpace(nugetExe))
            {
                throw new InvalidOperationException("The nuget.exe path is not set");
            }

            if (!File.Exists(nugetExe))
            {
                throw new InvalidOperationException($"The nuget.exe path '{nugetExe}' does not exist");
            }

            string packageSourceAppSettingsKey = ConfigurationConstants.NuGetPackageSourceName;

            string packageSource = StaticKeyValueConfigurationManager.AppSettings[packageSourceAppSettingsKey];

            var args = new List<string> { "list", "-prerelease" };

            if (!string.IsNullOrWhiteSpace(packageSource))
            {
                logger?.Debug("Using package source '{PackageSource}'", packageSource);
                args.Add("-source");
                args.Add(packageSource);
                args.Add("-AllVersions");
            }
            else
            {
                logger?.Debug(
                    "There is no package source defined i app settings, key '{PackageSourceAppSettingsKey}', using all sources",
                    packageSourceAppSettingsKey);
            }

            string configFile = StaticKeyValueConfigurationManager.AppSettings[ConfigurationConstants.NugetConfigFile];

            if (configFile.HasValue() && File.Exists(configFile))
            {
                args.Add("-ConfigFile");
                args.Add(configFile);
            }

            var builder = new List<string>();
            var errorBuild = new List<string>();

            logger?.Debug("Running NuGet to find packages");

            ExitCode exitCode =
                await ProcessRunner.ExecuteAsync(nugetExe,
                    arguments: args,
                    standardOutLog: (s, t) => builder.Add(s),
                    standardErrorAction: (s, t) => errorBuild.Add(s),
                    toolAction: (s, t) => _logger.Information("{ProcessToolMessage}", s),
                    verboseAction: (s, t) => _logger.Verbose("{ProcessToolMessage}", s));

            string standardOut = string.Join(Environment.NewLine, builder);
            string standardErrorOut = string.Join(Environment.NewLine, errorBuild);

            if (!exitCode.IsSuccess)
            {
                var sources = new List<string>();
                var sourcesError = new List<string>();

                await ProcessRunner.ExecuteAsync(nugetExe,
                    arguments: new[] { "sources" },
                    standardOutLog: (s, t) => sources.Add(s),
                    standardErrorAction: (s, t) => sourcesError.Add(s),
                    toolAction: (s, t) => _logger.Information("{ProcessToolMessage}", s),
                    verboseAction: (s, t) => _logger.Verbose("{ProcessToolMessage}", s));

                string sourcesOut = string.Join(Environment.NewLine, sources);
                string sourcesErrorOut = string.Join(Environment.NewLine, sourcesError);

                throw new InvalidOperationException(
                    $"Exit code {exitCode.Code} when running NuGet list packages; standard out '{standardOut}', standard error '{standardErrorOut}', exe path '{nugetExe}', arguments '{string.Join(" ", args)}', nuget sources '{sourcesOut}', sources error '{sourcesErrorOut}'");
            }

            var ignoredOutputStatements = new List<string> { "Using credentials" };

            List<string> filtered =
                (!string.IsNullOrWhiteSpace(prefix)
                    ? builder.Where(line =>
                        line.IndexOf(prefix, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList()
                    : builder).ToList();

            List<string> included =
                filtered.Where(line => !ignoredOutputStatements.Any(ignored =>
                        line.IndexOf(ignored, StringComparison.InvariantCultureIgnoreCase) >= 0))
                    .ToList();

            List<PackageVersion> items = included.Select(
                    package =>
                    {
                        string[] parts = package.Split(' ');

                        string packageId = parts.First();

                        try
                        {
                            string version = parts.Last();

                            if (!SemanticVersion.TryParse(version, out SemanticVersion semanticVersion))
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

            if (InMemoryCache.All.SequenceEqual(items))
            {
                return InMemoryCache.All;
            }

            InMemoryCache.Invalidate();

            var addedPackages = new List<string>();

            foreach (PackageVersion packageVersion in items)
            {
                addedPackages.Add(packageVersion.ToString());

                InMemoryCache.Add(packageVersion);
            }

            if (Log.Logger.IsEnabled(LogEventLevel.Verbose))
            {
                Log.Logger.Verbose("Added {Count} packages to in-memory cache {PackageVersions}",
                    addedPackages.Count,
                    addedPackages);
            }
            else
            {
                Log.Logger.Information("Added {Count} packages to in-memory cache",
                    addedPackages.Count);
            }

            return items;
        }

        public Task RefreshPackagesAsync()
        {
            return GetPackageVersionsAsync(useCache: false);
        }

        private static string LogJobMetadata(
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
                metadata.AppendLine($"Target URI: {deploymentTarget.Uri}");
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

        private static DirectoryInfo EnsureDeploymentJobsDirectoryExists()
        {
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "App_Data");

            string baseDir = StaticKeyValueConfigurationManager.AppSettings["urn:milou:deployer:jobs-directory"]
                .WithDefault(directoryPath);

            string jobDirectoryPath = Path.Combine(baseDir, "DeploymentJobs");

            var directoryInfo = new DirectoryInfo(jobDirectoryPath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            return directoryInfo;
        }

        private static void CheckPackageMatchingTarget(DeploymentTarget deploymentTarget, string packageId)
        {
            if (deploymentTarget.AllowedPackageNames.Any(name =>
                name.Equals("*", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            if (
                !deploymentTarget.AllowedPackageNames.Any(
                    allowed => allowed.Equals(packageId,
                        StringComparison.InvariantCultureIgnoreCase)))
            {
                string allPackageIds = string.Join(", ",
                    deploymentTarget.AllowedPackageNames.Select(name => $"'{name}'"));

                throw new InvalidOperationException(
                    $"The package id '{packageId}' is not in the list of allowed package ids: {allPackageIds}");
            }
        }

        private async Task<ExitCode> RunDeploymentToolAsync(
            DeploymentTask deploymentTask,
            DirectoryInfo deploymentJobsDirectory,
            DeploymentTarget deploymentTarget,
            ILogger logger)
        {
            string contentFilePath = GetMainLogFilePath(deploymentTask,
                deploymentJobsDirectory);

            ExitCode exitCode;

            using (Logger log = new LoggerConfiguration()
                .WriteTo.File(contentFilePath)
                .WriteTo.DelegateSink(deploymentTask.Log)
                .CreateLogger())
            {
                logger.Debug(
                    "Running tool '{Deployer}' for deployment target '{DeploymentTarget}', package '{PackageId}' version {Version}",
                    _deployer,
                    deploymentTarget,
                    deploymentTask.PackageId,
                    deploymentTask.SemanticVersion.ToNormalizedString());

                exitCode = await _deployer.ExecuteAsync(deploymentTask, log);
            }

            return exitCode;
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
            if (deploymentTarget.AllowedPackageNames.Any())
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
                    $"Deployment target with id '{deploymentTask.DeploymentTargetId}' was not found");
            }

            return deploymentTarget;
        }
    }
}