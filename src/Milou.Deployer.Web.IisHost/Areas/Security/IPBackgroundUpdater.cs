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
    public class IPBackgroundUpdater : BackgroundService
    {
        private readonly AllowedIPAddressHandler _ipHandler;
        private readonly ILogger _logger;

        public IPBackgroundUpdater(
            [NotNull] AllowedIPAddressHandler ipHandler,
            [NotNull] ILogger logger)
        {
            _ipHandler = ipHandler ?? throw new ArgumentNullException(nameof(ipHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (string domain in _ipHandler.Domains)
                {
                    IPHostEntry ipHostEntry = await Dns.GetHostEntryAsync(domain);

                    if (ipHostEntry.AddressList.Length == 1)
                    {
                        IPAddress ip = ipHostEntry.AddressList[0];

                        if (!_ipHandler.SetDomainIP(domain, ip))
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