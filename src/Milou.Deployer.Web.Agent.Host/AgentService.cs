using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Serilog;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Milou.Deployer.Web.Agent.Host
{
    public class AgentService : BackgroundService
    {
        private readonly IDeploymentPackageAgent _deploymentPackageAgent;
        private readonly ILogger _logger;

        private HubConnection _hubConnection;
        string _accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJhZ2VudDEiLCJ1bmlxdWVfbmFtZSI6ImFnZW50MSIsIm5iZiI6MTU4MjMxNjczNSwiZXhwIjoxNjcyNDQxMjAwLCJpYXQiOjE1ODIzMTY3MzV9.Ct3y__VNYl2ZBhD24lLRKNRnauKgBm2Ma9T-HxOed8Q";

        public AgentService(IDeploymentPackageAgent deploymentPackageAgent, ILogger logger)
        {
            _deploymentPackageAgent = deploymentPackageAgent;
            _logger = logger;
        }

        private async Task ExecuteDeploymentTask(string deploymentTaskId, string deploymentTargetId)
        {
            if (string.IsNullOrWhiteSpace(deploymentTaskId))
            {
                return;
            }

            var exitCode = await _deploymentPackageAgent.RunAsync(deploymentTaskId, deploymentTargetId);

            if (!exitCode.IsSuccess)
            {
                //TODO replace with http call
                await _hubConnection.InvokeAsync("DeployFailed", deploymentTaskId, deploymentTargetId);
            }
            else
            {
                //TODO replace with http call
                await _hubConnection.InvokeAsync("DeploySucceeded", deploymentTaskId, deploymentTargetId);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("Starting Agent service {Service}", nameof(AgentService));

            await Task.Yield();

            string connectionUrl = "http://localhost:34343/agents";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(connectionUrl, options =>
                {
                    options.AccessTokenProvider = GetAccessToken;
                })
                .Build();

            _hubConnection.Closed += HubConnectionOnClosed;

            _hubConnection.On<string, string>("Deploy", ExecuteDeploymentTask);

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.ReadJwtToken(_accessToken);
            string agentId =  jwtSecurityToken.Claims.SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.UniqueName)?.Value;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                bool connected = false;

                while (!connected)
                {
                    try
                    {
                        _logger.Debug("Connecting to server");
                        await _hubConnection.StartAsync(stoppingToken);
                        await _hubConnection.SendAsync("AgentConnect", stoppingToken);
                        connected = true;
                        _logger.Debug("Connected to server");
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        _logger.Error(ex, "Could not connect to server from agent {Agent}", agentId);

                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.Error(ex, "Could not connect to server from agent {Agent}", agentId);
            }

            _logger.Debug("Agent background service waiting for cancellation");
            await stoppingToken;
            _logger.Debug("Cancellation requested in Agent app");
            _logger.Debug("Stopping SignalR in Agent");

            await _hubConnection.StopAsync();

            _logger.Debug("Stopped SignalR");
        }

        private async Task<string> GetAccessToken()
        {
            return _accessToken;
        }

        private async Task HubConnectionOnClosed(Exception arg)
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await _hubConnection.StartAsync();
        }
    }
}