using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Processing;
using Newtonsoft.Json;
using Serilog;

namespace Milou.Deployer.Web.Core.Deployment
{
    [UsedImplicitly]
    public class MilouDeployer
    {
        private readonly ICredentialReadService _credentialReadService;
        private readonly IKeyValueConfiguration _keyValueConfiguration;
        private readonly IDeploymentTargetReadService _deploymentTargetReadService;
        private readonly MilouDeployerConfiguration _milouDeployerConfiguration;

        public MilouDeployer(
            [NotNull] MilouDeployerConfiguration milouDeployerConfiguration,
            [NotNull] IDeploymentTargetReadService deploymentTargetReadService,
            [NotNull] ICredentialReadService credentialReadService,
            [NotNull] IKeyValueConfiguration keyValueConfiguration)
        {
            _milouDeployerConfiguration = milouDeployerConfiguration ??
                                          throw new ArgumentNullException(nameof(milouDeployerConfiguration));
            _deploymentTargetReadService = deploymentTargetReadService ??
                                           throw new ArgumentNullException(nameof(deploymentTargetReadService));
            _credentialReadService =
                credentialReadService ?? throw new ArgumentNullException(nameof(credentialReadService));
            _keyValueConfiguration = keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
        }

        public async Task<ExitCode> ExecuteAsync(
            DeploymentTask deploymentTask,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            string jobId = "MDep_" + Guid.NewGuid();

            logger.Information("Starting job {JobId}", jobId);

            DeploymentTarget deploymentTarget = await GetDeploymentTarget(deploymentTask.DeploymentTargetId, cancellationToken);

            FileInfo exeFile = CheckExecutableExists();

            Environment.SetEnvironmentVariable(ConfigurationConstants.AllowPreReleaseEnabled,
                "true"); // TODO try to remove

            SetLogging();

            if (exeFile.Directory is null)
            {
                throw new InvalidOperationException("Invalid file directory");
            }

            using (CurrentDirectoryContext.Create(exeFile.Directory))
            {
                string targetDirectoryPath = GetTargetDirectoryPath(deploymentTarget, jobId, deploymentTask);

                string targetEnvironmentConfigName =
                    deploymentTarget.EnvironmentConfiguration;

                var arguments = new List<string>();

                logger.Information("Using manifest file for job {JobId}", jobId);

                string publishSettingsFile = deploymentTarget.PublishSettingFile;

                bool useManifest = bool.TryParse(
                                       _keyValueConfiguration[
                                           ConfigurationConstants.DeployerManifestEnabled],
                                       out bool useManitest) && useManitest;

                string deploymentTargetParametersFile = deploymentTarget.ParameterFile;

                var tempManifestFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"{jobId}.manifest"));

                deploymentTask.TempFiles.Add(tempManifestFile);

                if (useManifest)
                {
                    ImmutableDictionary<string, string[]> parameterDictionary;

                    if (!string.IsNullOrWhiteSpace(deploymentTargetParametersFile) &&
                        File.Exists(deploymentTargetParametersFile))
                    {
                        string parametersJson = File.ReadAllText(deploymentTargetParametersFile, Encoding.UTF8);

                        parameterDictionary = JsonConvert
                            .DeserializeObject<Dictionary<string, string[]>>(parametersJson).ToImmutableDictionary();

                        logger.Information("Using WebDeploy parameters from file {DeploymentTargetParametersFile}",
                            deploymentTargetParametersFile);
                    }
                    else
                    {
                        logger.Information("No WebDeploy parameters file exists ('{DeploymentTargetParametersFile}')",
                            deploymentTargetParametersFile);

                        parameterDictionary = deploymentTarget.Parameters;
                    }

                    ImmutableDictionary<string, string[]> parameters = parameterDictionary;

                    if (deploymentTarget.PublishSettingsXml.HasValue())
                    {
                        string tempFileName = Path.GetTempFileName();

                        await File.WriteAllTextAsync(tempFileName, deploymentTarget.PublishSettingsXml, Encoding.UTF8, cancellationToken);

                        deploymentTask.TempFiles.Add(new FileInfo(tempFileName));

                        publishSettingsFile = tempFileName;
                    }

                    if (!File.Exists(publishSettingsFile))
                    {
                        const string secretKeyPrefix = "publish-settings";

                        string id = deploymentTarget.Id;

                        string usernameKey = secretKeyPrefix + ":username";
                        string passwordKey = secretKeyPrefix + ":password";
                        string publishUrlKey = secretKeyPrefix + ":publish-url";
                        string msdeploySiteKey = secretKeyPrefix + ":msdeploySite";

                        string username = _credentialReadService.GetSecretAsync(id, usernameKey);
                        string password = _credentialReadService.GetSecretAsync(id, passwordKey);
                        string publishUrl = _credentialReadService.GetSecretAsync(id, publishUrlKey);
                        string msdeploySite = _credentialReadService.GetSecretAsync(id, msdeploySiteKey);

                        if (StringUtils.AllHaveValues(username, password, publishUrl, msdeploySite))
                        {
                            FileInfo fileInfo = CreateTempPublishFile(deploymentTarget,
                                username,
                                password,
                                publishUrl);

                            deploymentTask.TempFiles.Add(fileInfo);

                            publishSettingsFile = fileInfo.FullName;
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
                                iisSiteName = deploymentTarget.IisSiteName
                            }
                        }
                    };

                    string json = JsonConvert.SerializeObject(definitions, Formatting.Indented);

                    logger.Information("Using definitions JSON: {Json}", json);

                    logger.Information("Using temp manifest file '{ManifestFile}'", tempManifestFile.FullName);

                    await File.WriteAllTextAsync(tempManifestFile.FullName, json, Encoding.UTF8, cancellationToken);

                    arguments.Add($"\"{tempManifestFile.FullName}\"");
                }
                else
                {
                    logger.Information("Using direct arguments for job {JobId}", jobId);

                    arguments = new List<string>
                    {
                        deploymentTask.PackageId,
                        deploymentTask.SemanticVersion.ToNormalizedString(),
                        targetDirectoryPath,
                        deploymentTask.SemanticVersion.IsPrerelease.ToString()
                    };

                    if (!string.IsNullOrWhiteSpace(targetEnvironmentConfigName))
                    {
                        arguments.Add(targetEnvironmentConfigName);
                    }
                }

                ExitCode milouExitCode = await ProcessRunner.ExecuteAsync(
                    _milouDeployerConfiguration.MilouDeployerExePath,
                    logger,
                    arguments,
                    cancellationToken: cancellationToken);

                ClearTemporaryDirectoriesAndFiles(deploymentTask);

                if (!milouExitCode.IsSuccess)
                {
                    return milouExitCode;
                }

                return ExitCode.Success;
            }
        }

        public async Task<DeploymentTarget> GetDeploymentTarget([NotNull] string deploymentTargetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(deploymentTargetId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(deploymentTargetId));
            }

            DeploymentTarget deploymentTarget =
                await _deploymentTargetReadService.GetDeploymentTargetAsync(deploymentTargetId, cancellationToken);

            return deploymentTarget;
        }

        private static string GetTargetDirectoryPath(
            DeploymentTarget deploymentTarget,
            string jobId,
            DeploymentTask deploymentTask)
        {
            var targetTempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "MDep", jobId));

            string targetDirectoryPath = deploymentTarget.TargetDirectory
                .WithDefault(targetTempDirectory.FullName);

            if (!Directory.Exists(targetDirectoryPath))
            {
                Directory.CreateDirectory(targetDirectoryPath);
            }

            deploymentTask.TempDirectories.Add(targetTempDirectory);

            return targetDirectoryPath;
        }

        private FileInfo CreateTempPublishFile(
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

            string tempFileName = Path.GetTempFileName();

            using (var fileStream = new FileStream(tempFileName, FileMode.Open, FileAccess.Write))
            {
                doc.Save(fileStream);
            }

            return new FileInfo(tempFileName);
        }

        private void ClearTemporaryDirectoriesAndFiles(DeploymentTask deploymentTask)
        {
            foreach (FileInfo temporaryFile in deploymentTask.TempFiles)
            {
                temporaryFile.Refresh();

                if (temporaryFile.Exists)
                {
                    temporaryFile.Delete();
                }
            }

            foreach (DirectoryInfo deploymentTaskTempDirectory in deploymentTask.TempDirectories)
            {
                deploymentTaskTempDirectory.Refresh();

                if (deploymentTaskTempDirectory.Exists)
                {
                    deploymentTaskTempDirectory.Delete(true);
                }
            }
        }

        [NotNull]
        private FileInfo CheckExecutableExists()
        {
            var exeFile = new FileInfo(_milouDeployerConfiguration.MilouDeployerExePath);

            if (!exeFile.Exists)
            {
                throw new InvalidOperationException(
                    $"The file '{_milouDeployerConfiguration.MilouDeployerExePath}' does not exist");
            }

            return exeFile;
        }

        private void SetLogging()
        {
            if (!string.IsNullOrEmpty(_milouDeployerConfiguration.LogLevel))
            {
                Environment.SetEnvironmentVariable("loglevel", _milouDeployerConfiguration.LogLevel);
            }
        }
    }
}