using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Application;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    [UsedImplicitly]
    public class ContainerModeConfigurator : IConfigureEnvironment
    {
        private readonly IKeyValueConfiguration _keyValueConfiguration;

        public ContainerModeConfigurator(IKeyValueConfiguration keyValueConfiguration)
        {
            _keyValueConfiguration = keyValueConfiguration;
        }

        public void Configure(EnvironmentConfiguration environmentConfiguration)
        {
            string proxiesValue = _keyValueConfiguration[ApplicationConstants.ProxyAddresses].WithDefault("");

            ImmutableArray<IPAddress> proxies = proxiesValue.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(ipString =>
                    (Result: IPAddress.TryParse((string)ipString, out IPAddress address), IpAddress: address))
                .Where(address => address.Result)
                .Select(address => address.IpAddress)
                .ToImmutableArray();

            environmentConfiguration.ProxyAddresses.AddRange(proxies);

            if (int.TryParse(_keyValueConfiguration[ApplicationConstants.ProxyForwardLimit], out int proxyLimit) &&
                proxyLimit > 0)
            {
                environmentConfiguration.ForwardLimit = proxyLimit;
            }

            if (bool.TryParse(_keyValueConfiguration[ApplicationConstants.DotnetRunningInContainer],
                    out bool runningInContainer) && runningInContainer)
            {
                if (!environmentConfiguration.ForwardLimit.HasValue)
                {
                    var limit = environmentConfiguration.ProxyAddresses.Count + 1;
                    environmentConfiguration.ForwardLimit = limit;
                }
            }
        }
    }
}