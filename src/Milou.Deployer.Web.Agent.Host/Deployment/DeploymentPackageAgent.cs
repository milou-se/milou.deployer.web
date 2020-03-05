using System;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions.Time;
using Arbor.Processing;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.Agent.Host.Configuration;
using Milou.Deployer.Web.Agent.Host.Logging;
using Serilog;

namespace Milou.Deployer.Web.Agent.Host.Deployment
{
    public class DeploymentPackageAgent : IDeploymentPackageAgent
    {
        private readonly AgentConfiguration _agentConfiguration;
        private readonly IDeploymentPackageHandler _deploymentPackageHandler;
        private readonly DeploymentTaskPackageService _deploymentTaskPackageService;
        private readonly ILogger _logger;
        private readonly LogHttpClientFactory _logHttpClientFactory;
        private readonly TimeoutHelper _timeoutHelper;

        public DeploymentPackageAgent(
            TimeoutHelper timeoutHelper,
            ILogger logger,
            LogHttpClientFactory logHttpClientFactory,
            IDeploymentPackageHandler deploymentPackageHandler,
            DeploymentTaskPackageService deploymentTaskPackageService,
            AgentConfiguration agentConfiguration)
        {
            _timeoutHelper = timeoutHelper;
            _logger = logger;
            _logHttpClientFactory = logHttpClientFactory;
            _deploymentPackageHandler = deploymentPackageHandler;
            _deploymentTaskPackageService = deploymentTaskPackageService;
            _agentConfiguration = agentConfiguration;
        }

        public async Task<ExitCode> RunAsync(string deploymentTaskId,
            string deploymentTargetId,
            CancellationToken cancellationToken)
        {
            _logger.Information("Received deployment task {DeploymentTaskId}", deploymentTaskId);

            var client = _logHttpClientFactory.CreateClient(deploymentTaskId, deploymentTargetId);

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Logger(_logger)
                .WriteTo.DurableHttpUsingTimeRolledBuffers(AgentConstants.DeploymentTaskLogRoute,
                    period: TimeSpan.FromSeconds(5), httpClient: client)
                .CreateLogger(); //TODO create job logger in agent

            ExitCode exitCode;

            try
            {
                using var cancellationTokenSource =
                    _timeoutHelper.CreateCancellationTokenSource(TimeSpan.FromMinutes(30));

                DeploymentTaskPackage deploymentTaskPackage =
                    await _deploymentTaskPackageService.GetDeploymentTaskPackageAsync(deploymentTaskId,
                        cancellationTokenSource.Token);

                if (deploymentTaskPackage is null)
                {
                    _logger.Error("Could not get deployment task package for deployment task id {DeploymentTaskId}",
                        deploymentTaskId);

                    return ExitCode.Failure;
                }

                if (string.IsNullOrWhiteSpace(deploymentTaskPackage.DeploymentTaskId))
                {
                    _logger.Error(
                        "Deployment task package for deployment task id {DeploymentTaskId} is missing deployment task id",
                        deploymentTaskId);

                    return ExitCode.Failure;
                }

                if (string.IsNullOrWhiteSpace(deploymentTaskPackage.DeploymentTargetId))
                {
                    _logger.Error(
                        "Deployment task package for deployment task id {DeploymentTaskId} is missing deployment target id",
                        deploymentTaskId);

                    return ExitCode.Failure;
                }

                exitCode =
                    await _deploymentPackageHandler.RunAsync(deploymentTaskPackage, logger,
                        cancellationTokenSource.Token);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Error(ex, "Failed to deploy {DeploymentTaskId}", deploymentTaskId);
                return ExitCode.Failure;
            }

            return exitCode;
        }
    }
}