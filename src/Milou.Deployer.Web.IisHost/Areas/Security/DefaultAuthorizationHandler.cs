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
        private HashSet<string> _whiteListed;

        public DefaultAuthorizationHandler(IKeyValueConfiguration keyValueConfiguration)
        {
            string[] ipAddressesFromConfig = keyValueConfiguration[ConfigurationConstants.WhiteListedIPs]
                .Split(',', StringSplitOptions.RemoveEmptyEntries);

            _whiteListed = new HashSet<string> { "::1", "127.0.0.1" };

            foreach (string address in ipAddressesFromConfig)
            {
                _whiteListed.Add(address);
            }
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DefaulAuthorizationRequrement requirement)
        {
            if (context.User.HasClaim(claim =>
                claim.Type == CustomClaimTypes.IPAddress &&
                _whiteListed.Any(ip => claim.Value.StartsWith(ip, StringComparison.OrdinalIgnoreCase))))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}