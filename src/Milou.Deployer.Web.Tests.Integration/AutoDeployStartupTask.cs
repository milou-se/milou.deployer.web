using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.IisHost.Areas.Application;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;
using Milou.Deployer.Web.Tests.Integration.TestData;
using Serilog;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class AutoDeployStartupTask : BackgroundService, IStartupTask
    {
        private readonly DeploymentService _deploymentService;
        private readonly ILogger _logger;
        private readonly IDeploymentTargetReadService _readService;
        private readonly TestConfiguration _testConfiguration;
        private readonly PortPoolRental _testSiteHttpPort;
        private IWebHost _webHost;

        public AutoDeployStartupTask(
            DeploymentService deploymentService,
            PortPoolRental testSiteHttpPort,
            ILogger logger,
            IDeploymentTargetReadService readService,
            TestConfiguration testConfiguration = null)
        {
            _deploymentService = deploymentService;
            _testConfiguration = testConfiguration;
            _testSiteHttpPort = testSiteHttpPort;
            _logger = logger;
            _readService = readService;
        }

        public bool IsCompleted { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken startupCancellationToken)
        {
            if (_testConfiguration is null)
            {
                IsCompleted = true;
                return;
            }

            var targets = await _readService.GetDeploymentTargetsAsync(startupCancellationToken);

            if (targets.Length != 1)
            {
                throw new DeployerAppException("The test target has not been created");
            }

            const string packageVersion = "MilouDeployerWebTest 1.2.4";

            var deploymentTaskId = Guid.NewGuid();
            const string deploymentTargetId = TestDataCreator.Testtarget;
            var deploymentTask = new DeploymentTask(packageVersion, deploymentTargetId, deploymentTaskId);

            var deploymentTaskResult = await _deploymentService.ExecuteDeploymentAsync(
                deploymentTask,
                _logger,
                startupCancellationToken);

            if (!deploymentTaskResult.ExitCode.IsSuccess)
            {
                throw new DeployerAppException(
                    $"Initial deployment failed, metadata: {deploymentTaskResult.Metadata}; test configuration: {_testConfiguration}");
            }

            _webHost = WebHost.CreateDefaultBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback,
                        _testSiteHttpPort.Port);
                })
                .UseContentRoot(_testConfiguration.SiteAppRoot.FullName)
                .UseStartup<TestStartup>().Build();

            await _webHost.StartAsync(startupCancellationToken);

            startupCancellationToken.Register(() => _webHost.StopAsync(startupCancellationToken));
            HttpResponseMessage response;

            using (var httpClient = new HttpClient())
            {
                response = await httpClient.GetAsync(
                    $"http://localhost:{_testSiteHttpPort.Port}/applicationmetadata.json",
                    startupCancellationToken);
            }

            IsCompleted = true;
        }
    }
}
