using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Core.Extensions;
using Milou.Deployer.Web.Core.Application;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.AspNetCore.Startup
{
    [UsedImplicitly]
    public class ApplicationEnvironmentConfigurator : IConfigureEnvironment
    {
        private readonly IKeyValueConfiguration _keyValueConfiguration;

        public ApplicationEnvironmentConfigurator([NotNull] IKeyValueConfiguration keyValueConfiguration)
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

            if (int.TryParse(_keyValueConfiguration[ApplicationConstants.PublicPort], out var port))
            {
                environmentConfiguration.PublicPort = port;
            }

            if (bool.TryParse(_keyValueConfiguration[ApplicationConstants.PublicPortIsHttps], out var isHttps))
            {
                environmentConfiguration.PublicPortIsHttps = isHttps;
            }

            if (int.TryParse(_keyValueConfiguration[ApplicationConstants.HttpPort], out var httpPort) && port >= 0)
            {
                environmentConfiguration.HttpPort = httpPort;
            }
            else if (bool.TryParse(_keyValueConfiguration[ApplicationConstants.UseExplicitPorts],
                out var useExplicitPorts))
            {
                environmentConfiguration.UseExplicitPorts = useExplicitPorts;
            }

            if (int.TryParse(_keyValueConfiguration[ApplicationConstants.HttpsPort], out var httpsPort) &&
                httpsPort >= 0)
            {
                environmentConfiguration.HttpsPort = httpsPort;
            }

            if (int.TryParse(_keyValueConfiguration[ApplicationConstants.ProxyForwardLimit], out var proxyLimit) &&
                proxyLimit >= 0)
            {
                environmentConfiguration.ForwardLimit = proxyLimit;
            }

            var pfxFile = _keyValueConfiguration[ApplicationConstants.PfxFile];
            var pfxPassword = _keyValueConfiguration[ApplicationConstants.PfxPassword];

            if (!string.IsNullOrWhiteSpace(pfxFile) && File.Exists(pfxFile))
            {
                environmentConfiguration.PfxFile = pfxFile;
            }

            if (!string.IsNullOrWhiteSpace(pfxPassword))
            {
                environmentConfiguration.PfxPassword = pfxPassword;
            }
        }
    }
}
