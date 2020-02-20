using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions.Time;
using Arbor.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Milou.Deployer.Web.Core.Credentials;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.Core.Deployment.Sources;
using Serilog;
using Serilog.Core;

namespace Milou.Deployer.Web.Agent.Host
{
    public class AgentApp
    {
        public async Task<ExitCode> RunAsync()
        {
            LoggingLevelSwitch levelSwitch = new LoggingLevelSwitch();
            var logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .CreateLogger();

            var host = new HostBuilder().ConfigureServices((hostContext, services) =>
                {
                    services.Configure<HostOptions>(option =>
                    {
                        services.AddSingleton<ICredentialReadService, CredentialReadProxyService>();
                        services.AddSingleton<IDeploymentTargetService, DeploymentTargetProxyService>();
                        services.AddSingleton<ICustomClock,CustomSystemClock>();
                        services.AddSingleton(levelSwitch);
                        services.AddSingleton(logger);
                        services.AddSingleton(new DeploymentServiceSettings() {PublishEventEnabled = false});
                        services.AddSingleton<MilouDeployer>();
                        services.AddSingleton<DeploymentService>();
                        services.AddHttpClient();
                        services.AddHostedService<AgentService>();
                    });
                })
                .Build();

            await host.RunAsync();

            return ExitCode.Success;
        }

        public static async Task<ExitCode> CreateAndRunAsync(string[] args)
        {
            var app = new AgentApp();

            return await app.RunAsync();
        }
    }

    public class CredentialReadProxyService : ICredentialReadService
    {
        public string GetSecret(string id, string secretKey, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }

    public class DeploymentTargetProxyService :IDeploymentTargetService
    {
        public Task<DeploymentTarget> GetDeploymentTargetAsync(string deploymentTargetId, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }
}