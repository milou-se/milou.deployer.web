using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Urns;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Milou.Deployer.Web.Core.Deployment.WorkTasks;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.Core.Startup;
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
        private IWebHost _webHost;
        private readonly TestHttpPort _testSiteHttpPort;

        public AutoDeployStartupTask(
            DeploymentService deploymentService,
            ILogger logger,
            IDeploymentTargetReadService readService,
            ConfigurationInstanceHolder configurationInstanceHolder,
            TestConfiguration testConfiguration = null)
        {
            _deploymentService = deploymentService;
            _testConfiguration = testConfiguration;
            _testSiteHttpPort = configurationInstanceHolder.GetInstances<TestHttpPort>().Values.SingleOrDefault();
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
            var deploymentTask = new DeploymentTask(packageVersion, deploymentTargetId, deploymentTaskId, nameof(AutoDeployStartupTask));

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
                .ConfigureServices(services => services.AddSingleton(_testConfiguration))
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback,
                        _testSiteHttpPort.Port.Port + 1);
                })
                .UseContentRoot(_testConfiguration.SiteAppRoot.FullName)
                .UseStartup<TestStartup>().Build();

            await _webHost.StartAsync(startupCancellationToken);

            startupCancellationToken.Register(() => _webHost.StopAsync(startupCancellationToken));
            HttpResponseMessage response = default;

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var uri = new Uri($"http://localhost:{_testSiteHttpPort.Port.Port + 1}/applicationmetadata.json");
                    response = await httpClient.GetAsync(uri, startupCancellationToken);

                    _logger.Information("Successfully made get request to test site {Status}", response.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not get successful http get response in integration test, {Status}", response?.StatusCode);
                }
            }

            IsCompleted = true;
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            base.Dispose();
            _webHost.SafeDispose();
            _testSiteHttpPort.SafeDispose();
        }
    }
}
