using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arbor.AspNetCore.Host.Hosting
{
    public static class CustomWebHostExtensions
    {
        public static async Task WaitForShutdownAsync(this IHost host)
        {
            var applicationLifetime = host.Services.GetService<IHostApplicationLifetime>();

            var waitForStop = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            applicationLifetime.ApplicationStopping.Register(obj =>
            {
                var tcs = (TaskCompletionSource<object>)obj;
                tcs.TrySetResult(null);
            }, waitForStop);

            await waitForStop.Task;

            await host.StopAsync();
        }
    }
}