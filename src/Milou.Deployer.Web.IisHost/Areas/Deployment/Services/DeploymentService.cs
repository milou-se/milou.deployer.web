using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.Processing;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Time;
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
        private readonly ICustomClock _customClock;
        private readonly MilouDeployer _deployer;
        private readonly IKeyValueConfiguration _keyValueConfiguration;
        private readonly ILogger _logger;
        private readonly LoggingLevelSwitch _loggingLevelSwitch;
        private readonly IMediator _mediator;

        private readonly IDeploymentTargetReadService _targetSource;

        public DeploymentService(
            [NotNull] ILogger logger,
            [NotNull] IDeploymentTargetReadService targetSource,
            [NotNull] IMediator mediator,
            [NotNull] MilouDeployer deployer,
            [NotNull] IKeyValueConfiguration keyValueConfiguration,
            [NotNull] ICustomClock customClock,
            [NotNull] LoggingLevelSwitch loggingLevelSwitch)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _targetSource = targetSource ?? throw new ArgumentNullException(nameof(targetSource));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _deployer = deployer ?? throw new ArgumentNullException(nameof(deployer));
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
            _customClock = customClock ?? throw new ArgumentNullException(nameof(customClock));
            _loggingLevelSwitch = loggingLevelSwitch ?? throw new ArgumentNullException(nameof(loggingLevelSwitch));
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

            metadata
                .Append("Started job ")
                .Append(deploymentTask.DeploymentTaskId)
                .Append(" at ")
                .AppendFormat(CultureInfo.InvariantCulture, "{0:O}", start)
                .Append(" and finished at ")
                .AppendFormat(CultureInfo.InvariantCulture, "{0:O}", end).AppendLine();

            metadata
                .Append("Total time ")
                .AppendFormat(CultureInfo.InvariantCulture, "{0:f}", stopwatch.Elapsed.TotalSeconds)
                .AppendLine(" seconds");

            metadata
                .Append("Package version: ")
                .Append(deploymentTask.SemanticVersion)
                .AppendLine();

            metadata
                .Append("Package id: ")
                .AppendLine(deploymentTask.PackageId);

            metadata
                .Append("Target id: ")
                .AppendLine(deploymentTask.DeploymentTargetId);

            if (deploymentTarget is null)
            {
                metadata.AppendLine("Deployment target not found");
            }
            else
            {
                metadata.Append("Publish settings file: ").AppendLine(deploymentTarget.PublishSettingFile);
                metadata.Append("Target directory: ").AppendLine(deploymentTarget.TargetDirectory);
                metadata.Append("Target URI: ").Append(deploymentTarget.Url).AppendLine();
            }

            metadata.Append("Exit code ").Append(exitCode).AppendLine();

            var metadataContent = metadata.ToString();

            var metadataFilePath = Path.Combine(deploymentJobsDirectory.FullName,
                $"{deploymentTask.DeploymentTaskId}.metadata.txt");

            File.WriteAllText(metadataFilePath, metadataContent, Encoding.UTF8);

            return metadataContent;
        }

        private static string GetMainLogFilePath(DeploymentTask deploymentTask, DirectoryInfo deploymentJobsDirectory)
        {
            var contentFilePath = Path.Combine(deploymentJobsDirectory.FullName,
                $"{deploymentTask.DeploymentTaskId}.txt");
            return contentFilePath;
        }

        private static void CheckPackageMatchingTarget(DeploymentTarget deploymentTarget, string packageId)
        {
            if (
                !deploymentTarget.PackageId.Equals(packageId,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                var allPackageIds = string.Join(", ",
                    deploymentTarget.PackageId.Select(name => $"'{name}'"));

                throw new DeployerAppException(
                    $"The package id '{packageId}' is not in the list of allowed package ids: {allPackageIds}");
            }
        }

        private static void VerifyPreReleaseAllowed(
            SemanticVersion version,
            DeploymentTarget deploymentTarget,
            string packageId,
            ILogger logger)
        {
            if (version.IsPrerelease && !deploymentTarget.AllowPreRelease)
            {
                throw new DeployerAppException(
                    $"Could not deploy package with id '{packageId}' to target '{deploymentTarget}' because the package is a pre-release version and the target does not support it");
            }

            if (version.IsPrerelease && logger.IsEnabled(LogEventLevel.Debug))
            {
                logger.Debug(
                    "The deployment target '{DeploymentTarget}' allows package id '{PackageId}' version {Version}, pre-release",
                    deploymentTarget,
                    packageId,
                    version.ToNormalizedString());
            }
        }

        private static void VerifyAllowedPackageIsAllowed(
            DeploymentTarget deploymentTarget,
            string packageId,
            ILogger logger)
        {
            if (logger.IsEnabled(LogEventLevel.Debug))
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
        }

        private DirectoryInfo EnsureDeploymentJobsDirectoryExists()
        {
            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "App_Data");

            var baseDir = _keyValueConfiguration["urn:milou:deployer:jobs-directory"]
                .WithDefault(directoryPath);

            var jobDirectoryPath = Path.Combine(baseDir, "DeploymentJobs");

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
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var contentFilePath = GetMainLogFilePath(deploymentTask,
                deploymentJobsDirectory);

            ExitCode exitCode;

            var logBuilder = new StringBuilder();

            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.File(contentFilePath)
                .WriteTo.DelegateSink(deploymentTask.Log)
                .WriteTo.DelegateSink(message => logBuilder.AppendLine(message))
                .WriteTo.Logger(logger);

            if (Debugger.IsAttached)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Debug();
            }

            loggerConfiguration = loggerConfiguration.MinimumLevel.ControlledBy(_loggingLevelSwitch);

            using (var log = loggerConfiguration.CreateLogger())
            {
                if (logger.IsEnabled(LogEventLevel.Debug))
                {
                    logger.Debug(
                        "Running tool '{Deployer}' for deployment target '{DeploymentTarget}', package '{PackageId}' version {Version}",
                        _deployer,
                        deploymentTarget,
                        deploymentTask.PackageId,
                        deploymentTask.SemanticVersion.ToNormalizedString());
                }

                try
                {
                    exitCode = await _deployer.ExecuteAsync(deploymentTask, log, _loggingLevelSwitch, logger, cancellationToken);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.Error(ex, "Failed to deploy task {DeploymentTask}", deploymentTask);
                    exitCode = ExitCode.Failure;
                }
            }

            var finishedAtUtc = _customClock.UtcNow().UtcDateTime;

            await _mediator.Publish(
                new DeploymentFinishedNotification(deploymentTask, logBuilder.ToString(), finishedAtUtc),
                cancellationToken);

            return (exitCode, finishedAtUtc);
        }

        public async Task<DeploymentTaskResult> ExecuteDeploymentAsync(
            [NotNull] DeploymentTask deploymentTask,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var start = _customClock.UtcNow().UtcDateTime;
            var stopwatch = Stopwatch.StartNew();

            (ExitCode, DateTime) result;

            var deploymentJobsDirectory = EnsureDeploymentJobsDirectoryExists();

            DeploymentTarget deploymentTarget = null;

            try
            {
                deploymentTarget = await _targetSource.GetDeploymentTargetAsync(deploymentTask.DeploymentTargetId,
                    cancellationToken);

                VerifyPreReleaseAllowed(deploymentTask.SemanticVersion,
                    deploymentTarget,
                    deploymentTask.PackageId,
                    logger);

                VerifyAllowedPackageIsAllowed(deploymentTarget, deploymentTask.PackageId, logger);

                result = await RunDeploymentToolAsync(deploymentTask,
                    deploymentJobsDirectory,
                    deploymentTarget,
                    logger,
                    cancellationToken);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                result = (ExitCode.Failure, _customClock.UtcNow().UtcDateTime);
                logger.Error(ex, "Error deploying");
            }

            stopwatch.Stop();

            var metadataContent = LogJobMetadata(deploymentTask,
                start,
                result.Item2,
                stopwatch,
                result.Item1,
                deploymentJobsDirectory,
                deploymentTarget);

            var deploymentTaskResult = new DeploymentTaskResult(deploymentTask.DeploymentTaskId,
                deploymentTask.DeploymentTargetId,
                result.Item1,
                start,
                result.Item2,
                metadataContent);

            await _mediator.Publish(new DeploymentMetadataLogNotification(deploymentTask, deploymentTaskResult),
                cancellationToken);

            return deploymentTaskResult;
        }
    }
}
