using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Arbor.Processing;
using JetBrains.Annotations;
using Milou.Deployer.Bootstrapper.Common;
using Milou.Deployer.Core;
using Milou.Deployer.Core.Configuration;
using Milou.Deployer.Web.Core.Credentials;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.IO;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace Milou.Deployer.Web.Core.Deployment
{
    [UsedImplicitly]
    public class MilouDeployer
    {
        private readonly ICredentialReadService _credentialReadService;

        private readonly IDeploymentTargetReadService _deploymentTargetReadService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly LoggingLevelSwitch _loggingLevelSwitch;

        public MilouDeployer(
            [NotNull] IDeploymentTargetReadService deploymentTargetReadService,
            [NotNull] ICredentialReadService credentialReadService,
            [NotNull] IHttpClientFactory clientFactory,
            [NotNull] LoggingLevelSwitch loggingLevelSwitch)
        {
            _deploymentTargetReadService = deploymentTargetReadService ??
                                           throw new ArgumentNullException(nameof(deploymentTargetReadService));
            _credentialReadService =
                credentialReadService ?? throw new ArgumentNullException(nameof(credentialReadService));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _loggingLevelSwitch = loggingLevelSwitch ?? throw new ArgumentNullException(nameof(loggingLevelSwitch));

            //_clientFactory = clientFactory;
        }

        private async Task<DeploymentTarget> GetDeploymentTarget(
            [NotNull] string deploymentTargetId,
            CancellationToken cancellationToken = default)
        {
            var deploymentTarget =
                await _deploymentTargetReadService.GetDeploymentTargetAsync(deploymentTargetId, cancellationToken);

            return deploymentTarget;
        }

        private static string GetTargetDirectoryPath(
            DeploymentTarget deploymentTarget,
            string jobId,
            DeploymentTask deploymentTask)
        {
            var targetTempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "MDep", jobId));

            var targetDirectoryPath = deploymentTarget.TargetDirectory
                .WithDefault(targetTempDirectory.FullName);

            if (!Directory.Exists(targetDirectoryPath))
            {
                Directory.CreateDirectory(targetDirectoryPath);
            }

            deploymentTask.TempDirectories.Add(targetTempDirectory);

            return targetDirectoryPath;
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

        private static void ClearTemporaryDirectoriesAndFiles(DeploymentTask deploymentTask)
        {
            if (deploymentTask is null)
            {
                return;
            }

            foreach (var temporaryFile in deploymentTask.TempFiles)
            {
                temporaryFile.SafeDispose();
            }

            foreach (var deploymentTaskTempDirectory in deploymentTask.TempDirectories)
            {
                deploymentTaskTempDirectory.Refresh();


                if (deploymentTaskTempDirectory.Exists)
                {
                    deploymentTaskTempDirectory.Delete(true);
                }
            }
        }

        private static void SetLogging(LoggingLevelSwitch loggingLevelSwitch)
        {
            Environment.SetEnvironmentVariable("loglevel", loggingLevelSwitch.MinimumLevel.ToString());
        }

        public async Task<ExitCode> ExecuteAsync(
            DeploymentTask deploymentTask,
            ILogger jobLogger,
            LoggingLevelSwitch loggingLevelSwitch,
            ILogger mainLogger,
            CancellationToken cancellationToken = default)
        {
            var jobId = "MDep_" + Guid.NewGuid();

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

            var targetDirectoryPath = GetTargetDirectoryPath(deploymentTarget, jobId, deploymentTask);

            var targetEnvironmentConfigName =
                deploymentTarget.EnvironmentConfiguration;

            var arguments = new List<string>();

            jobLogger.Information("Using manifest file for job {JobId}", jobId);

            var publishSettingsFile = deploymentTarget.PublishSettingFile;

            var deploymentTargetParametersFile = deploymentTarget.ParameterFile;

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
                var parametersJson =
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

            var parameters = parameterDictionary;

            if (deploymentTarget.PublishSettingsXml.HasValue())
            {
                var tempFileName = TempFile.CreateTempFile();

                var expandedXml = Environment.ExpandEnvironmentVariables(deploymentTarget.PublishSettingsXml);

                await File.WriteAllTextAsync(tempFileName.File.FullName,
                    expandedXml,
                    Encoding.UTF8,
                    cancellationToken);

                deploymentTask.TempFiles.Add(tempFileName);

                publishSettingsFile = tempFileName.File.FullName;
            }

            if (!File.Exists(publishSettingsFile))
            {
                const string secretKeyPrefix = "publish-settings";

                var id = deploymentTarget.Id;

                const string usernameKey = secretKeyPrefix + ":username";
                const string passwordKey = secretKeyPrefix + ":password";
                const string publishUrlKey = secretKeyPrefix + ":publish-url";
                const string msdeploySiteKey = secretKeyPrefix + ":msdeploySite";

                var username = _credentialReadService.GetSecret(id, usernameKey);
                var password = _credentialReadService.GetSecret(id, passwordKey);
                var publishUrl = _credentialReadService.GetSecret(id, publishUrlKey);
                var msdeploySite = _credentialReadService.GetSecret(id, msdeploySiteKey);

                if (StringUtils.AllHaveValues(username, password, publishUrl, msdeploySite))
                {
                    var tempPublishFile = CreateTempPublishFile(deploymentTarget,
                        username,
                        password,
                        publishUrl);

                    deploymentTask.TempFiles.Add(tempPublishFile);

                    publishSettingsFile = tempPublishFile.File.FullName;
                }
                else
                {
                    Log.Warning("Could not get secrets for deployment target id {DeploymentTargetId}", id);
                }
            }

            var definitions = new
            {
                definitions = new object[]
                {
                    new
                    {
                        deploymentTask.PackageId,
                        targetDirectoryPath,
                        isPreRelease = deploymentTask.SemanticVersion.IsPrerelease,
                        environmentConfig = targetEnvironmentConfigName,
                        publishSettingsFile,
                        parameters,
                        deploymentTarget.NuGetConfigFile,
                        deploymentTarget.NuGetPackageSource,
                        semanticVersion = deploymentTask.SemanticVersion.ToNormalizedString(),
                        iisSiteName = deploymentTarget.IisSiteName,
                        webConfigTransform = deploymentTarget.WebConfigTransform,
                        publishType = deploymentTarget.PublishType.Name,
                        ftpPath = deploymentTarget.FtpPath?.Path
                    }
                }
            };

            var json = JsonConvert.SerializeObject(definitions, Formatting.Indented);

            jobLogger.Information("Using definitions JSON: {Json}", json);

            jobLogger.Information("Using temp manifest file '{ManifestFile}'", tempManifestFile.File.FullName);

            await File.WriteAllTextAsync(tempManifestFile.File.FullName, json, Encoding.UTF8, cancellationToken);

            arguments.Add(tempManifestFile.File.FullName);
            arguments.Add(Bootstrapper.Common.Constants.AllowPreRelease);
            arguments.Add(LoggingConstants.PlainOutputFormatEnabled);
            arguments.Add($"{ConfigurationKeys.LogLevelEnvironmentVariable}={_loggingLevelSwitch.MinimumLevel}");

            var deployerArgs = arguments.ToArray();

            jobLogger.Verbose("Running Milou Deployer bootstrapper");

            HttpClient httpClient = _clientFactory.CreateClient("Bootstrapper");

            try
            {
                mainLogger.Debug("Starting milou deployer process with args {Args}", string.Join(" ", deployerArgs));

                using (var deployerApp =
                    await App.CreateAsync(deployerArgs,
                        jobLogger,
                        httpClient,
                        false,
                        cancellationToken))
                {
                    var result = await deployerApp.ExecuteAsync(
                        deployerArgs.ToImmutableArray(),
                        cancellationToken);

                    if (result.PackageDirectory is null || result.SemanticVersion is null)
                    {
                        jobLogger.Warning("Milou.Deployer failed");
                        return ExitCode.Failure;
                    }
                }
            }
            finally
            {
                ClearTemporaryDirectoriesAndFiles(deploymentTask);
            }

            return ExitCode.Success;
        }
    }
}
