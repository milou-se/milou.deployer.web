using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Milou.Deployer.Web.Core.Deployment;
using Milou.Deployer.Web.IisHost.Areas.Deployment.Services;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Middleware
{
    [UsedImplicitly]
    public class JobLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly DeploymentWorker _worker;

        public JobLogMiddleware(RequestDelegate next, DeploymentWorker worker)
        {
            _next = next;
            _worker = worker;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Query.ContainsKey("targetid"))
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await StartAsync(context, webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        private async Task StartAsync(HttpContext context, WebSocket webSocket)
        {
            if (!context.Request.Query.TryGetValue("targetid", out StringValues value))
            {
                return;
            }

            string targetId = value;

            var buffer = new byte[1024 * 4];

            int delayInMilliseconds = 300;
            int maxWaitTimeoutInMilliseconds = 20 * delayInMilliseconds;

            using (var blockingQueue = new BlockingCollection<string>())
            {
                DeploymentTargetWorker targetWorker = _worker.Worker(targetId);

                if (targetWorker != null)
                {
                    int currentDelay = 0;

                    DeploymentTask currentJob = null;

                    while (currentJob == null && currentDelay <= maxWaitTimeoutInMilliseconds)
                    {
                        currentJob = targetWorker.CurrentTask;

                        if (currentJob == null)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));
                            currentDelay += delayInMilliseconds;
                        }
                    }

                    if (currentJob is null)
                    {
                        try
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                "Timeout, no work was scheduled during wait period",
                                CancellationToken.None);
                        }
                        catch (WebSocketException ex)
                        {
                            Serilog.Log.Error(ex, "Web socket exception on close");
                        }

                        return;
                    }

                    void Action(string message, WorkTaskStatus status)
                    {
                        blockingQueue.Add(message);

                        if (status == WorkTaskStatus.Done || status == WorkTaskStatus.Failed)
                        {
                            blockingQueue.CompleteAdding();
                        }
                    }

                    currentJob.LogActions.Add(Action);

                    while (!webSocket.CloseStatus.HasValue && !blockingQueue.IsCompleted)
                    {
                        string message = blockingQueue.Take(CancellationToken.None);

                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);

                        bytes.CopyTo(buffer, 0);

                        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytes.Length),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }

                    currentJob.LogActions.Remove(Action);

                    try
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                            "Done, task completed",
                            CancellationToken.None);
                    }
                    catch (WebSocketException ex)
                    {
                        Serilog.Log.Error(ex, "Web socket exception on close");
                    }
                }
                else
                {
                    try
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                            "No worker found",
                            CancellationToken.None);
                    }
                    catch (WebSocketException ex)
                    {
                        Serilog.Log.Error(ex, "Web socket exception on close");
                    }
                }
            }
        }
    }
}