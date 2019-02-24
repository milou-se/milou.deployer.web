﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Milou.Deployer.Web.IisHost.Areas.Network;
using Serilog;
using Serilog.Events;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [UsedImplicitly]
    public class DefaultAuthorizationHandler : AuthorizationHandler<DefaultAuthorizationRequirement>
    {
        private readonly HashSet<IPAddress> _allowed;
        private readonly ImmutableArray<AllowedEmail> _allowedEmails;
        private readonly ImmutableArray<AllowedEmailDomain> _allowedEmailDomains;
        private readonly AllowedIPAddressHandler _allowedIPAddressHandler;
        private readonly ImmutableHashSet<IPNetwork> _allowedNetworks;
        private readonly ILogger _logger;

        public DefaultAuthorizationHandler(
            IKeyValueConfiguration keyValueConfiguration,
            AllowedIPAddressHandler allowedIPAddressHandler,
            ILogger logger,
            IReadOnlyCollection<AllowedEmail> allowedEmails,
            ImmutableArray<AllowedEmailDomain> allowedEmailDomains)
        {
            _allowedIPAddressHandler = allowedIPAddressHandler;
            _logger = logger;
            _allowedEmailDomains = allowedEmailDomains;
            _allowedEmails = allowedEmails.SafeToImmutableArray();

            IPAddress[] ipAddressesFromConfig = keyValueConfiguration[ConfigurationConstants.AllowedIPs]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(IPAddress.Parse)
                .ToArray();

            IPNetwork[] ipNetworksFromConfig = keyValueConfiguration[ConfigurationConstants.AllowedIPNetworks]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(network => (HasValue: IpNetworkParser.TryParse(network, out IPNetwork ipNetwork), ipNetwork))
                .Where(network => network.HasValue)
                .Select(network => network.ipNetwork)
                .ToArray();

            _allowedNetworks = ipNetworksFromConfig.ToImmutableHashSet();

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
            if (!context.User.Claims.Any())
            {
                return Task.CompletedTask;
            }

            Claim[] emailClaims =
                context.User.Claims.Where(claim => claim.Type.Equals(ClaimTypes.Email, StringComparison.Ordinal))
                    .ToArray();

            if (emailClaims.Length > 0)
            {
                if (_allowedEmailDomains.Length > 0)
                {
                    var matches = emailClaims.Select(claim =>
                        {
                            bool parsed = EmailAddress.TryParse(claim.Value, out EmailAddress parsedAddress);

                            if (!parsed)
                            {
                                return null;
                            }

                            return parsedAddress;
                        }).Where(emailAddress => emailAddress != null)
                        .Where(emailAddress => _allowedEmailDomains.Any(domain =>
                            domain.Domain.Equals(emailAddress.Domain, StringComparison.OrdinalIgnoreCase)))
                        .ToArray();

                    if (matches.Length > 0)
                    {
                        if (_logger.IsEnabled(LogEventLevel.Verbose))
                        {
                            _logger.Verbose("User has allowed email domain, authorized");
                        }

                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }


                Claim[] allowedEmails = emailClaims.Where(emailClaim =>
                        _allowedEmails.Any(allowed => emailClaim.Value.Equals(allowed.Email, StringComparison.Ordinal)))
                    .ToArray();

                if (allowedEmails.Length > 0)
                {
                    if (_logger.IsEnabled(LogEventLevel.Verbose))
                    {
                        _logger.Verbose("User has allowed email claim, authorized");
                    }

                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            string ipClaimValue = context.User.Claims.SingleOrDefault(claim =>
                claim.Type.Equals(CustomClaimTypes.IPAddress, StringComparison.Ordinal))?.Value;

            if (string.IsNullOrWhiteSpace(ipClaimValue))
            {
                if (_logger.IsEnabled(LogEventLevel.Verbose))
                {
                    _logger.Verbose("User has no claim of type {ClaimType}", CustomClaimTypes.IPAddress);
                }

                return Task.CompletedTask;
            }

            if (!IPAddress.TryParse(ipClaimValue, out IPAddress address))
            {
                if (_logger.IsEnabled(LogEventLevel.Verbose))
                {
                    _logger.Verbose(
                        "User has claim of type {ClaimType}, but the value {IpClaimValue} is not a valid IP address",
                        CustomClaimTypes.IPAddress, ipClaimValue);
                }

                return Task.CompletedTask;
            }

            IPNetwork[] ipNetworks = _allowedNetworks.Where(network => network.Contains(address)).ToArray();

            if (ipNetworks.Length > 0)
            {
                if (_logger.IsEnabled(LogEventLevel.Verbose))
                {
                    string networks = string.Join(", ",
                        ipNetworks.Select(network => $"{network.Prefix}/{network.PrefixLength}"));
                    _logger.Verbose("User claim ip address {Address} is in allowed networks {Networks}", address,
                        networks);
                }

                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            ImmutableArray<IPAddress> dynamicIPAddresses = _allowedIPAddressHandler.IpAddresses;

            ImmutableHashSet<IPAddress> allAddresses = _allowed
                .Concat(dynamicIPAddresses)
                .Where(ip => !Equals(ip, IPAddress.None))
                .ToImmutableHashSet();

            if (allAddresses.Any(current => current.EqualsAddress(address)))
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