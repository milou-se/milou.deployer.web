using System;
using System.Threading;
using System.Threading.Tasks;
using Arbor.App.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;

namespace Milou.Deployer.Web.Agent.Host
{
    public class AgentService : BackgroundService
    {
        private HubConnection _hubConnection;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            string connectionUrl = "http://localhost:34343/agent";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(connectionUrl)
                .Build();

            _hubConnection.Closed += HubConnectionOnClosed;

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {

            });

            try
            {
                await _hubConnection.StartAsync(stoppingToken);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
            }

            await stoppingToken;

            await _hubConnection.StopAsync();
        }

        private async Task HubConnectionOnClosed(Exception arg)
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await _hubConnection.StartAsync();
        }
    }
}