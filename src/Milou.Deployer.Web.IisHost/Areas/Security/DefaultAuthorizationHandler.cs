using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [UsedImplicitly]
    public class DefaultAuthorizationHandler : AuthorizationHandler<DefaulAuthorizationRequrement>
    {
        private HashSet<string> _allowed;

        public DefaultAuthorizationHandler(IKeyValueConfiguration keyValueConfiguration)
        {
            string[] ipAddressesFromConfig = keyValueConfiguration[ConfigurationConstants.AllowedIPs]
                .Split(',', StringSplitOptions.RemoveEmptyEntries);

            _allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "::1", "127.0.0.1" };

            foreach (string address in ipAddressesFromConfig)
            {
                _allowed.Add(address);
            }
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DefaulAuthorizationRequrement requirement)
        {
            if (context.User.HasClaim(claim =>
                claim.Type == CustomClaimTypes.IPAddress
                && _allowed.Any(ip => claim.Value.StartsWith(ip, StringComparison.OrdinalIgnoreCase))))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}