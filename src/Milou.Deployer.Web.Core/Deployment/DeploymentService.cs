using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Arbor.App.Extensions;
using Arbor.App.Extensions.IO;
using Arbor.App.Extensions.Time;
using Arbor.Processing;
using JetBrains.Annotations;
using MediatR;
using Milou.Deployer.Core.Configuration;
using Milou.Deployer.Core.Logging;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Credentials;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Messages;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.Targets;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.Core.Logging;
using Newtonsoft.Json;
using NuGet.Versioning;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Milou.Deployer.Web.Agent
{
    [UsedImplicitly]
    public class DeploymentService : IDeploymentService
    {
        private readonly ICustomClock _customClock;

        private readonly ILogger _logger;
        private readonly LoggingLevelSwitch _loggingLevelSwitch;
        private readonly IMediator _mediator;

        private readonly IDeploymentTargetService _targetSource;
        private readonly ICredentialReadService _credentialReadService;
        private readonly IDeploymentTargetService _deploymentTargetService;
        private IAgentService _agentService;

        public DeploymentService(
            [NotNull] ILogger logger,
            [NotNull] IDeploymentTargetService targetSource,
            [NotNull] IMediator mediator,
            [NotNull] ICustomClock customClock,
            [NotNull] LoggingLevelSwitch loggingLevelSwitch,
            ICredentialReadService credentialReadService,
            IDeploymentTargetService deploymentTargetService,
            IAgentService agentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _targetSource = targetSource ?? throw new ArgumentNullException(nameof(targetSource));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            _customClock = customClock ?? throw new ArgumentNullException(nameof(customClock));
            _loggingLevelSwitch = loggingLevelSwitch ?? throw new ArgumentNullException(nameof(loggingLevelSwitch));
            _credentialReadService = credentialReadService;
            _deploymentTargetService = deploymentTargetService;
            _agentService = agentService;
        }

        private static string LogJobMetadata(
            DeploymentTask deploymentTask,
            DateTime start,
            DateTime end,
            Stopwatch stopwatch,
            ExitCode exitCode,
            DeploymentTarget? deploymentTarget)
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

            string metadataContent = metadata.ToString();

            return metadataContent;
        }

        private static void CheckPackageMatchingTarget(DeploymentTarget deploymentTarget, string packageId)
        {
            if (
                !deploymentTarget.PackageId.Equals(packageId,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                string allPackageIds = string.Join(", ",
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

        private async Task<ExitCode> PrepareDeploymentAsync(
            DeploymentTask deploymentTask,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            ExitCode exitCode;

            var logBuilder = new List<LogItem>();

            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.DelegateSink((message, level) => deploymentTask.Log(message), _loggingLevelSwitch.MinimumLevel)
                .WriteTo.DelegateSink((message, level) =>
                        logBuilder.Add(new LogItem
                        {
                            Message = message, Level = (int)level, TimeStamp = _customClock.UtcNow()
                        }),
                    LogEventLevel.Verbose)
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
                        "Preparing deploy {TaskId} for deployment target '{DeploymentTarget}', package '{PackageId}' version {Version}",
                     deploymentTask.DeploymentTaskId,
                        deploymentTask.DeploymentTargetId,
                        deploymentTask.PackageId,
                        deploymentTask.SemanticVersion.ToNormalizedString());
                }

                try
                {

                   exitCode = await CreateDeploymentPackageAsync(deploymentTask, log, _loggingLevelSwitch, logger, cancellationToken);

                    //    exitCode = await _deployer.CreateDeploymentPackageAsync(deploymentTask, log, _loggingLevelSwitch, logger, cancellationToken);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.Error(ex, "Failed to deploy task {DeploymentTask}", deploymentTask);
                    exitCode = ExitCode.Failure;
                }
            }

            //var finishedAtUtc = _customClock.UtcNow().UtcDateTime;

            //if (_deploymentServiceSettings.PublishEventEnabled)
            //{
            //    await _mediator.Publish(
            //        new DeploymentFinishedNotification(deploymentTask, logBuilder.ToArray(), finishedAtUtc),
            //        cancellationToken);
            //}

            return exitCode;
        }

        private static void SetLogging(LoggingLevelSwitch loggingLevelSwitch) =>
            Environment.SetEnvironmentVariable("loglevel", loggingLevelSwitch.MinimumLevel.ToString());

        public async Task<ExitCode> CreateDeploymentPackageAsync(
            DeploymentTask deploymentTask,
            ILogger jobLogger,
            LoggingLevelSwitch loggingLevelSwitch,
            ILogger mainLogger,
            CancellationToken cancellationToken = default)
        {
            string jobId = "MDep_" + Guid.NewGuid();

            jobLogger.Information("Starting job {JobId}", jobId);

            DeploymentTarget deploymentTarget;

            try
            {
                deploymentTarget =
                    await GetDeploymentTarget(deploymentTask.DeploymentTargetId, cancellationToken);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                jobLogger.Error("Could not get deployment target with id {Id}", deploymentTask.DeploymentTargetId);
                return ExitCode.Failure;
            }

            SetLogging(loggingLevelSwitch);

            string targetDirectoryPath = deploymentTarget.TargetDirectory;

            var targetEnvironmentConfig = deploymentTarget.GetEnvironmentConfiguration()?.Trim();

            var arguments = new List<string>();

            jobLogger.Information("Using manifest file for job {JobId}", jobId);

            FileInfo publishSettingsFile = !string.IsNullOrWhiteSpace(deploymentTarget.PublishSettingFile)
                ? new FileInfo(deploymentTarget.PublishSettingFile)
                : null;

            string publishSettingsXml = null;

            string deploymentTargetParametersFile = deploymentTarget.ParameterFile;

            var tempManifestFile = TempFile.CreateTempFile(jobId, ".manifest");

            deploymentTask.TempFiles.Add(tempManifestFile);

            ImmutableDictionary<string, string[]> parameterDictionary;

            if (!string.IsNullOrWhiteSpace(deploymentTargetParametersFile)
                && !Path.IsPathRooted(deploymentTargetParametersFile))
            {
                jobLogger.Error(
                    "The deployment target {DeploymentTarget} parameter file '{DeploymentTargetParametersFile}' is not a rooted path",
                    deploymentTarget,
                    deploymentTargetParametersFile);

                return ExitCode.Failure;
            }

            if (!string.IsNullOrWhiteSpace(deploymentTargetParametersFile)
                && File.Exists(deploymentTargetParametersFile))
            {
                string parametersJson =
                    await File.ReadAllTextAsync(deploymentTargetParametersFile, Encoding.UTF8, cancellationToken);

                parameterDictionary = JsonConvert
                    .DeserializeObject<Dictionary<string, string[]>>(parametersJson).ToImmutableDictionary();

                jobLogger.Information("Using WebDeploy parameters from file {DeploymentTargetParametersFile}",
                    deploymentTargetParametersFile);
            }
            else
            {
                jobLogger.Information("No WebDeploy parameters file exists ('{DeploymentTargetParametersFile}')",
                    deploymentTargetParametersFile);

                parameterDictionary = deploymentTarget.Parameters;
            }

            ImmutableDictionary<string, string[]> parameters = parameterDictionary;

            if (deploymentTarget.PublishSettingsXml.HasValue())
            {
                var tempFileName = TempFile.CreateTempFile();

                string expandedXml = Environment.ExpandEnvironmentVariables(deploymentTarget.PublishSettingsXml);

                await File.WriteAllTextAsync(tempFileName.File.FullName,
                    expandedXml,
                    Encoding.UTF8,
                    cancellationToken);

                deploymentTask.TempFiles.Add(tempFileName);

                publishSettingsFile = tempFileName.File;
            }

            if (publishSettingsFile?.Exists ?? false)
            {
                const string secretKeyPrefix = "publish-settings";

                string id = deploymentTarget.Id;

                const string usernameKey = secretKeyPrefix + ":username";
                const string passwordKey = secretKeyPrefix + ":password";
                const string publishUrlKey = secretKeyPrefix + ":publish-url";
                const string msdeploySiteKey = secretKeyPrefix + ":msdeploySite";

                string username = _credentialReadService.GetSecret(id, usernameKey);
                string password = _credentialReadService.GetSecret(id, passwordKey);
                string publishUrl = _credentialReadService.GetSecret(id, publishUrlKey);
                string msdeploySite = _credentialReadService.GetSecret(id, msdeploySiteKey);

                if (StringUtils.AllHaveValues(username, password, publishUrl, msdeploySite))
                {
                    TempFile tempPublishFile = CreateTempPublishFile(deploymentTarget,
                        username,
                        password,
                        publishUrl);

                    deploymentTask.TempFiles.Add(tempPublishFile);

                    publishSettingsFile = tempPublishFile.File;
                }
                else
                {
                    Log.Warning("Could not get secrets for deployment target id {DeploymentTargetId}", id);
                }
            }

            var publishSettingsFileName = publishSettingsFile is null
                ? null
                : $"{deploymentTask.DeploymentTargetId}.publishSettings";

            var definitions = new
            {
                definitions = new object[]
                {
                    new
                    {
                        deploymentTask.PackageId,
                        targetDirectoryPath,
                        isPreRelease = deploymentTask.SemanticVersion.IsPrerelease,
                        environmentConfig = targetEnvironmentConfig,
                        requireEnvironmentConfig = deploymentTarget.RequireEnvironmentConfiguration,
                        publishSettingsFileName,
                        parameters,
                        deploymentTarget.NuGet.NuGetConfigFile,
                        deploymentTarget.NuGet.NuGetPackageSource,
                        semanticVersion = deploymentTask.SemanticVersion.ToNormalizedString(),
                        iisSiteName = deploymentTarget.IisSiteName,
                        webConfigTransform = deploymentTarget.WebConfigTransform,
                        publishType = deploymentTarget.PublishType.Name,
                        ftpPath = deploymentTarget.FtpPath?.Path,
                        packageListPrefixEnabled = deploymentTarget.PackageListPrefixEnabled,
                        packageListPrefix = deploymentTarget.PackageListPrefixEnabled.HasValue && deploymentTarget.PackageListPrefixEnabled.Value ? deploymentTarget.PackageListPrefix : ""
                    }
                }
            };

            string nugetXml = null;

            if (publishSettingsFile?.Exists ?? false)
            {
                publishSettingsXml = await
                    File.ReadAllTextAsync(publishSettingsFile.FullName, Encoding.UTF8, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(deploymentTarget.NuGet.NuGetConfigFile) && File.Exists(deploymentTarget.NuGet.NuGetConfigFile))
            {
                nugetXml = await
                    File.ReadAllTextAsync(deploymentTarget.NuGet.NuGetConfigFile, Encoding.UTF8, cancellationToken);
            }

            string manifestJson = JsonConvert.SerializeObject(definitions, Formatting.Indented);

            jobLogger.Information("Using definitions JSON: {Json}", manifestJson);

            string manifestFile = $"manifest.json";
            jobLogger.Information("Using temp manifest file '{ManifestFile}'", manifestFile);

            arguments.Add(manifestFile);
            arguments.Add(Bootstrapper.Common.Constants.AllowPreRelease);
            arguments.Add(LoggingConstants.PlainOutputFormatEnabled);
            arguments.Add($"{ConfigurationKeys.LogLevelEnvironmentVariable}={_loggingLevelSwitch.MinimumLevel}");
            arguments.Add($"{LoggingConstants.LoggingCategoryFormatEnabled}");

            jobLogger.Verbose("Running Milou Deployer bootstrapper");

            try
            {
                // mainLogger.Debug("Starting milou deployer process with args {Args}", string.Join(" ", deployerArgs));

                DeploymentTaskPackage deploymentTaskPackage = new DeploymentTaskPackage(
                    deploymentTask.DeploymentTaskId,
                    deploymentTask.DeploymentTargetId,
                    arguments.ToImmutableArray(),
                    nugetXml,
                    manifestJson,
                    publishSettingsXml,
                    "");

                await _mediator.Send(new CreateDeploymentTaskPackage(deploymentTaskPackage), cancellationToken);
            }
            finally
            {
                //ClearTemporaryDirectoriesAndFiles(deploymentTask);
            }

            return ExitCode.Success;
        }
        private static TempFile CreateTempPublishFile(
            DeploymentTarget deploymentTarget,
            string username,
            string password,
            string publishUrl)
        {
            var doc = new XDocument();

            var profileNameAttribute = new XAttribute("profileName", deploymentTarget.Name);
            var publishMethodAttribute = new XAttribute("publishMethod", "MSDeploy");
            var publishUrlAttribute = new XAttribute("publishUrl", publishUrl);
            var userNameAttribute = new XAttribute("userName", username);
            var userPwdAttribute = new XAttribute("userPWD", password);
            var webSystemAttribute = new XAttribute("webSystem", "WebSites");
            var msdeploySiteAttribute = new XAttribute("msdeploySite", "WebSites");

            var publishProfile = new XElement(
                "publishProfile",
                profileNameAttribute,
                publishMethodAttribute,
                publishUrlAttribute,
                userNameAttribute,
                userPwdAttribute,
                webSystemAttribute,
                msdeploySiteAttribute);

            var root = new XElement("publishData", publishProfile);

            doc.Add(root);

            var tempFile = TempFile.CreateTempFile();

            using (var fileStream = new FileStream(tempFile.File.FullName, FileMode.Open, FileAccess.Write))
            {
                doc.Save(fileStream);
            }

            return tempFile;
        }

        private async Task<DeploymentTarget> GetDeploymentTarget(
            [NotNull] string deploymentTargetId,
            CancellationToken cancellationToken = default)
        {
            DeploymentTarget deploymentTarget =
                await _deploymentTargetService.GetDeploymentTargetAsync(deploymentTargetId, cancellationToken);

            return deploymentTarget;
        }

        public async Task<DeploymentTaskResult> ExecuteDeploymentAsync(
            [NotNull] DeploymentTask deploymentTask,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var start = _customClock.UtcNow().UtcDateTime;
            var stopwatch = Stopwatch.StartNew();

            ExitCode result;

            DateTime endUtc = default; //TODO
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

                result = await PrepareDeploymentAsync(deploymentTask,
                    logger,
                    cancellationToken);


                if (!result.IsSuccess)
                {
                    _logger.Error("Could not prepare deployment task {DeploymentTaskId}",
                        deploymentTask.DeploymentTaskId);
                }

                IDeploymentPackageAgent agent = await _agentService.GetAgentForDeploymentTask(deploymentTask, cancellationToken);

               var deployExitCode =  await agent.RunAsync(deploymentTask.DeploymentTaskId, cancellationToken);

               result = deployExitCode;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                result = ExitCode.Failure;
                logger.Error(ex, "Error deploying");
            }

            stopwatch.Stop();

            string metadataContent = LogJobMetadata(deploymentTask,
                start,
                endUtc,
                stopwatch,
                result,
                deploymentTarget);

            var deploymentTaskResult = new DeploymentTaskResult(deploymentTask.DeploymentTaskId,
                deploymentTask.DeploymentTargetId,
                result,
                start,
                endUtc,
                metadataContent);

                await _mediator.Publish(new DeploymentMetadataLogNotification(deploymentTask, deploymentTaskResult),
                    cancellationToken);


            return deploymentTaskResult;
        }
    }
}
