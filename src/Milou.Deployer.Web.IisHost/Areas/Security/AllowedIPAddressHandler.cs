using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using JetBrains.Annotations;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    public class AllowedIpAddressHandler
    {
        private static readonly ConcurrentTwoWaySingleValueMap<string, IPAddress> IpAddressMap =
            new ConcurrentTwoWaySingleValueMap<string, IPAddress>();

        public AllowedIpAddressHandler(
            [NotNull] IEnumerable<AllowedHostName> hostNames,
            [NotNull] ILogger logger)
        {
            if (hostNames == null)
            {
                throw new ArgumentNullException(nameof(hostNames));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            foreach (var allowedHostName in hostNames)
            {
                if (!IpAddressMap.TrySet(allowedHostName.HostName, IPAddress.None))
                {
                    logger.Verbose("Could not add allowed host name {HostName}", allowedHostName);
                }
            }
        }

        public ImmutableArray<string> Domains => IpAddressMap.ForwardKeys;

        public ImmutableArray<IPAddress> IpAddresses => IpAddressMap.ReverseKeys;

        public bool SetDomainIp([NotNull] string domain, [NotNull] IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(domain));
            }

            return IpAddressMap.TrySet(domain, ipAddress);
        }
    }
}
