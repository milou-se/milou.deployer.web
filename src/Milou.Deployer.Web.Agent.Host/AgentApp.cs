using System.Net.Http;
using System.Threading.Tasks;
using Arbor.App.Extensions.Time;
using Arbor.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Http;

namespace Milou.Deployer.Web.Agent.Host
{
    public class AgentApp
    {
        public async Task<ExitCode> RunAsync(string[] args)
        {
            LoggingLevelSwitch levelSwitch = new LoggingLevelSwitch()
            {
                MinimumLevel = LogEventLevel.Verbose
            };

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.ControlledBy(levelSwitch)
                .CreateLogger();

            logger.Debug("Started logging");

            var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
                {
                        services.AddSingleton(
                            new TimeoutHelper(new TimeoutConfiguration {CancellationEnabled = false}));
                        services.AddSingleton<DeploymentTaskPackageService>();
                        services.AddSingleton<LogHttpClientFactory>();
                        services.AddSingleton<IDeploymentPackageAgent, DeploymentPackageAgent>();
                        services.AddSingleton<IDeploymentPackageHandler, DeploymentPackageHandler>();
                        services.AddSingleton<ICustomClock, CustomSystemClock>();
                        services.AddSingleton(levelSwitch);
                        services.AddSingleton<ILogger>(logger);
                        services.AddSingleton(new DeploymentServiceSettings {PublishEventEnabled = false});

                        //services.AddSingleton<IDeploymentService, DeploymentService>();
                        services.AddHttpClient();
                        services.AddHostedService<AgentService>();

                })
                .Build();

            logger.Debug("Running host");
            await host.RunAsync();

            logger.Debug("Host shut down");

            return ExitCode.Success;
        }

        public static async Task<ExitCode> CreateAndRunAsync(string[] args)
        {
            var app = new AgentApp();

            return await app.RunAsync(args);
        }
    }

    public class LogHttpClientFactory
    {
        private readonly IHttpClientFactory _clientFactory;

        public LogHttpClientFactory(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IHttpClient CreateClient(string deploymentTaskId, string deploymentTargetId)
        {
            return new CustomHttpClient(_clientFactory, deploymentTaskId, deploymentTargetId);
        }
    }
}