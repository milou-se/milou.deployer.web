using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [UsedImplicitly]
    public class IpBackgroundUpdater : BackgroundService
    {
        private readonly AllowedIpAddressHandler _ipHandler;
        private readonly ILogger _logger;

        public IpBackgroundUpdater(
            [NotNull] AllowedIpAddressHandler ipHandler,
            [NotNull] ILogger logger)
        {
            _ipHandler = ipHandler ?? throw new ArgumentNullException(nameof(ipHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var domain in _ipHandler.Domains)
                {
                    var ipHostEntry = await Dns.GetHostEntryAsync(domain);

                    if (ipHostEntry.AddressList.Length == 1)
                    {
                        var ip = ipHostEntry.AddressList[0];

                        if (!_ipHandler.SetDomainIp(domain, ip))
                        {
                            _logger.Verbose("Could not update domain ip for host {Host}", domain);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
