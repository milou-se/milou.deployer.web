using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Serilog;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Milou.Deployer.Web.Agent.Host
{
    public class AgentService : BackgroundService, IAsyncDisposable
    {
        private readonly string _accessToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJhZ2VudDEiLCJ1bmlxdWVfbmFtZSI6ImFnZW50MSIsIm5iZiI6MTU4MjMxNjczNSwiZXhwIjoxNjcyNDQxMjAwLCJpYXQiOjE1ODIzMTY3MzV9.Ct3y__VNYl2ZBhD24lLRKNRnauKgBm2Ma9T-HxOed8Q"; // TODO make agent token configurable

        private readonly IDeploymentPackageAgent _deploymentPackageAgent;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        private HubConnection? _hubConnection;

        public AgentService(IDeploymentPackageAgent deploymentPackageAgent, ILogger logger, IMediator mediator)
        {
            _deploymentPackageAgent = deploymentPackageAgent;
            _logger = logger;
            _mediator = mediator;
        }

        private async Task ExecuteDeploymentTask(string deploymentTaskId, string deploymentTargetId)
        {
            if (string.IsNullOrWhiteSpace(deploymentTaskId))
            {
                return;
            }

            var exitCode = await _deploymentPackageAgent.RunAsync(deploymentTaskId, deploymentTargetId);

            var deploymentTaskAgentResult =
                new DeploymentTaskAgentResult(deploymentTaskId, deploymentTargetId, exitCode.IsSuccess);

            await _mediator.Send(deploymentTaskAgentResult);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("Starting Agent service {Service}", nameof(AgentService));

            await Task.Yield();

            string connectionUrl = "http://localhost:34343/agents"; //TODO make agent SignalR url configurable

            CreateSignalRConnection(connectionUrl);

            string agentId = GetAgentId();

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                bool connected = false;

                while (!connected && _hubConnection is {})
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

                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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

        }

        private void CreateSignalRConnection(string connectionUrl)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(connectionUrl, options => { options.AccessTokenProvider = GetAccessToken; })
                .Build();

            _hubConnection.Closed += HubConnectionOnClosed;

            _hubConnection.On<string, string>("Deploy", ExecuteDeploymentTask);
        }

        private string? GetAgentId()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.ReadJwtToken(_accessToken);

            string? agentId = jwtSecurityToken.Claims
                .SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.UniqueName)
                ?.Value;

            return agentId;
        }

        private async Task<string> GetAccessToken() => _accessToken;

        private async Task HubConnectionOnClosed(Exception arg)
        {
            if (_hubConnection is {})
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _hubConnection.StartAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is { })
            {
                await _hubConnection.StopAsync();

                _logger.Debug("Stopped SignalR");
            }
        }
    }
}