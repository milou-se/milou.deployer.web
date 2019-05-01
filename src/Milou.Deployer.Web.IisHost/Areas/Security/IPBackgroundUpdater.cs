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
        private readonly ILogger _logger;

        public IpBackgroundUpdater(
            [NotNull] ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var domain in AllowedIpAddressHandler.Domains)
                {
                    try
                    {
                        var ipHostEntry = await Dns.GetHostEntryAsync(domain);

                        if (ipHostEntry != null)
                        {
                            if (ipHostEntry.AddressList.Length == 1)
                            {
                                var ip = ipHostEntry.AddressList[0];

                                if (!AllowedIpAddressHandler.SetDomainIp(domain, ip))
                                {
                                    _logger.Verbose("Could not update domain ip for host {Host}", domain);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Verbose(ex, "Could not resolve domain {Domain}", domain);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
