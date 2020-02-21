using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.IO;
using Arbor.Processing;
using Arbor.Tooler;
using Milou.Deployer.Bootstrapper.Common;
using Serilog;

namespace Milou.Deployer.Web.Agent
{
    public class DeploymentPackageHandler : IDeploymentPackageHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeploymentPackageHandler(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        private static void ClearTemporaryDirectoriesAndFiles(IEnumerable<TempFile> tempFiles,IEnumerable<DirectoryInfo> tempDirectories)
        {
            foreach (TempFile temporaryFile in tempFiles)
            {
                temporaryFile.SafeDispose();
            }

            foreach (DirectoryInfo deploymentTaskTempDirectory in tempDirectories)
            {
                deploymentTaskTempDirectory.Refresh();


                if (deploymentTaskTempDirectory.Exists)
                {
                    deploymentTaskTempDirectory.Delete(true);
                }
            }
        }

        public async Task<ExitCode> RunAsync(
            DeploymentTaskPackage deploymentTaskPackage,
            ILogger jobLogger,
            CancellationToken cancellationToken)
        {
            using var manifestFile = TempFile.CreateTempFile("manifest", ".json");

            await File.WriteAllTextAsync(manifestFile.File.FullName, deploymentTaskPackage.ManifestJson, Encoding.UTF8, cancellationToken);

            using var publishSettings = string.IsNullOrWhiteSpace(deploymentTaskPackage.PublishSettingsXml)
                ? null
                : TempFile.CreateTempFile(deploymentTaskPackage.DeploymentTargetId, ".publishSettings");

            var currentDir = manifestFile.File!.Directory;

            if (publishSettings?.File?.Exists ?? false)
            {
                await File.WriteAllTextAsync(publishSettings.File.FullName, deploymentTaskPackage.PublishSettingsXml, Encoding.UTF8, cancellationToken);

                publishSettings.File.CopyTo(Path.Combine(currentDir.FullName, publishSettings.File.Name));
            }

            HttpClient httpClient = _httpClientFactory.CreateClient("Bootstrapper");

            Directory.SetCurrentDirectory(currentDir.FullName);

            using (App deployerApp =
                await App.CreateAsync(deploymentTaskPackage.DeployerProcessArgs.ToArray(),
                    jobLogger,
                    httpClient,
                    false,
                    cancellationToken))
            {
                NuGetPackageInstallResult result = await deployerApp.ExecuteAsync(
                    deploymentTaskPackage.DeployerProcessArgs,
                    cancellationToken);

                if (result.PackageDirectory is null || result.SemanticVersion is null)
                {
                    jobLogger.Warning("Milou.Deployer failed");
                    return ExitCode.Failure;
                }
            }

            return ExitCode.Success;
        }
    }
}