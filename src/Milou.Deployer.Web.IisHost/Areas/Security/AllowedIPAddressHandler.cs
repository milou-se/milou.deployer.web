using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using JetBrains.Annotations;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    public class AllowedIPAddressHandler
    {
        private static readonly ConcurrentTwoWaySingleValueMap<string, IPAddress> _IPAddresses =
            new ConcurrentTwoWaySingleValueMap<string, IPAddress>();

        public AllowedIPAddressHandler(
            [NotNull] IReadOnlyCollection<AllowedHostName> hostNames,
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

            foreach (AllowedHostName allowedHostName in hostNames)
            {
                if (!_IPAddresses.TrySet(allowedHostName.HostName, IPAddress.None))
                {
                    logger.Verbose("Could not add allowed host name {HostName}", allowedHostName);
                }
            }
        }

        public ImmutableArray<string> Domains => _IPAddresses.ForwardKeys;

        public ImmutableArray<IPAddress> IpAddresses => _IPAddresses.ReverseKeys;

        public bool SetDomainIP([NotNull] string domain, [NotNull] IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(domain));
            }

            return _IPAddresses.TrySet(domain, ipAddress);
        }
    }
}