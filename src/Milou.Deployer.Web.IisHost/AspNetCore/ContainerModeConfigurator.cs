using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [UsedImplicitly]
    public class ContainerModeConfigurator : IConfigureEnvironment
    {
        private readonly IKeyValueConfiguration _keyValueConfiguration;

        public ContainerModeConfigurator([NotNull] IKeyValueConfiguration keyValueConfiguration)
        {
            _keyValueConfiguration =
                keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
        }

        public void Configure([NotNull] EnvironmentConfiguration environmentConfiguration)
        {
            if (environmentConfiguration == null)
            {
                throw new ArgumentNullException(nameof(environmentConfiguration));
            }

            var proxiesValue = _keyValueConfiguration[ApplicationConstants.ProxyAddresses].WithDefault("");

            var proxies = proxiesValue.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(ipString =>
                    (HasIp: IPAddress.TryParse(ipString, out var address), IpAddress: address))
                .Where(address => address.HasIp)
                .Select(address => address.IpAddress)
                .ToImmutableArray();

            environmentConfiguration.ProxyAddresses.AddRange(proxies);

            environmentConfiguration.PublicHostname = _keyValueConfiguration[ApplicationConstants.PublicHostName];

            if (int.TryParse(_keyValueConfiguration[ApplicationConstants.PublicPort], out int port))
            {
                environmentConfiguration.PublicPort = port;
            }

            if (int.TryParse(_keyValueConfiguration[ApplicationConstants.ProxyForwardLimit], out var proxyLimit) &&
                proxyLimit >= 0)
            {
                environmentConfiguration.ForwardLimit = proxyLimit;
            }
        }
    }
}
