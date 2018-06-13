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
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Email;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Processing;
using Milou.Deployer.Web.IisHost.Areas.Logging;
using NuGet.Versioning;
using Serilog;
using Serilog.Core;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [UsedImplicitly]
    public class DeploymentService
    {
        private readonly MilouDeployer _deployer;
        private readonly IKeyValueConfiguration _keyValueConfiguration;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        private readonly IDeploymentTargetReadService _targetSource;
        private readonly ITime _time;

        public DeploymentService(
            [NotNull] ILogger logger,
            [NotNull] IDeploymentTargetReadService targetSource,
            [NotNull] IMediator mediator,
            [NotNull] MilouDeployer deployer,
            [NotNull] IKeyValueConfiguration keyValueConfiguration,
            ITime time)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _targetSource = targetSource ?? throw new ArgumentNullException(nameof(targetSource));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _deployer = deployer ?? throw new ArgumentNullException(nameof(deployer));
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
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