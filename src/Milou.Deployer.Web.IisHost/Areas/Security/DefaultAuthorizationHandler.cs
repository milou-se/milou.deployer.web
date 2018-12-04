using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Milou.Deployer.Web.Core.Configuration;
using Serilog;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [UsedImplicitly]
    public class DefaultAuthorizationHandler : AuthorizationHandler<DefaultAuthorizationRequirement>
    {
        private readonly HashSet<IPAddress> _allowed;
        private readonly AllowedIPAddressHandler _allowedIPAddressHandler;
        private readonly ILogger _logger;

        public DefaultAuthorizationHandler(
            IKeyValueConfiguration keyValueConfiguration,
            AllowedIPAddressHandler allowedIPAddressHandler,
            ILogger logger)
        {
            _allowedIPAddressHandler = allowedIPAddressHandler;
            _logger = logger;

            IPAddress[] ipAddressesFromConfig = keyValueConfiguration[ConfigurationConstants.AllowedIPs]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(IPAddress.Parse)
                .ToArray();

            _allowed = new HashSet<IPAddress> { IPAddress.Parse("::1"), IPAddress.Parse("127.0.0.1") };

            foreach (IPAddress address in ipAddressesFromConfig)
            {
                _allowed.Add(address);
            }
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DefaultAuthorizationRequirement requirement)
        {
            ImmutableArray<IPAddress> dynamicIPAddresses = _allowedIPAddressHandler.IpAddresses;

            ImmutableHashSet<IPAddress> allAddresses = _allowed
                .Concat(dynamicIPAddresses)
                .Where(ip => !Equals(ip, IPAddress.None))
                .ToImmutableHashSet();

            if (context.User.HasClaim(claim =>
                claim.Type == CustomClaimTypes.IPAddress
                && allAddresses.Any(ip => claim.Value.StartsWith(ip.ToString(), StringComparison.OrdinalIgnoreCase))))
            {
                if (_logger.IsEnabled(LogEventLevel.Verbose))
                {
                    _logger.Verbose("User has claim {ClaimType}", CustomClaimTypes.IPAddress);
                }

                context.Succeed(requirement);
            }
            else
            {
                if (_logger.IsEnabled(LogEventLevel.Verbose))
                {
                    _logger.Verbose("User does not have claim {ClaimType}", CustomClaimTypes.IPAddress);
                }
            }

            return Task.CompletedTask;
        }
    }
}