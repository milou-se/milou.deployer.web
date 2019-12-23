using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Core;
using Microsoft.AspNetCore.Authorization;
using Milou.Deployer.Web.Core.Security;
using Milou.Deployer.Web.IisHost.Areas.Security;
using Serilog.Core;
using Xunit;

namespace Milou.Deployer.Web.Tests.Integration
{
    public class DefaultAuthorizationHandlerTests
    {
        [Fact]
        public async Task IpClaimInRangeForAllowedNetworkShouldMarkContextSucceeded()
        {
            var logger = Logger.None;
            var nameValueCollection = new NameValueCollection
            {
                [ConfigurationConstants.AllowedIpNetworks] = "192.168.0.0/24"
            };
            var configuration = new InMemoryKeyValueConfiguration(nameValueCollection);

            var handler =
                new DefaultAuthorizationHandler(configuration,
                    logger,
                    ImmutableArray<AllowedEmail>.Empty,
                    ImmutableArray<AllowedEmailDomain>.Empty);

            IEnumerable<Claim> claims = new[] { new Claim(CustomClaimTypes.IpAddress, "192.168.0.2") };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var authorizationHandlerContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new DefaultAuthorizationRequirement() },
                user,
                null);

            await handler.HandleAsync(authorizationHandlerContext);

            Assert.True(authorizationHandlerContext.HasSucceeded);
        }

        [Fact]
        public async Task IpClaimInRangeForMultipleAllowedNetworksShouldMarkContextSucceeded()
        {
            var logger = Logger.None;
            var nameValueCollection = new NameValueCollection
            {
                [ConfigurationConstants.AllowedIpNetworks] = "192.168.0.0/24",
                [ConfigurationConstants.AllowedIpNetworks] = "192.168.0.0/16"
            };
            var configuration = new InMemoryKeyValueConfiguration(nameValueCollection);

            var handler =
                new DefaultAuthorizationHandler(configuration,
                    logger,
                    ImmutableArray<AllowedEmail>.Empty,
                    ImmutableArray<AllowedEmailDomain>.Empty);

            IEnumerable<Claim> claims = new[] { new Claim(CustomClaimTypes.IpAddress, "192.168.0.2") };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var authorizationHandlerContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new DefaultAuthorizationRequirement() },
                user,
                null);

            await handler.HandleAsync(authorizationHandlerContext);

            Assert.True(authorizationHandlerContext.HasSucceeded);
        }

        [Fact]
        public async Task IpClaimMissingShouldMarkContextSucceeded()
        {
            var logger = Logger.None;
            var nameValueCollection = new NameValueCollection
            {
                [ConfigurationConstants.AllowedIpNetworks] = "192.168.0.0/24"
            };
            var configuration = new InMemoryKeyValueConfiguration(nameValueCollection);

            var handler =
                new DefaultAuthorizationHandler(configuration,
                    logger,
                    ImmutableArray<AllowedEmail>.Empty,
                    ImmutableArray<AllowedEmailDomain>.Empty);

            IEnumerable<Claim> claims = ImmutableArray<Claim>.Empty;
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var authorizationHandlerContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new DefaultAuthorizationRequirement() },
                user,
                null);

            await handler.HandleAsync(authorizationHandlerContext);

            Assert.False(authorizationHandlerContext.HasSucceeded);
        }

        [Fact]
        public async Task IpClaimOutOfRangeForAllowedNetworkShouldMarkContextNotSucceeded()
        {
            var logger = Logger.None;
            var nameValueCollection = new NameValueCollection
            {
                [ConfigurationConstants.AllowedIpNetworks] = "192.168.0.0/24"
            };
            var configuration = new InMemoryKeyValueConfiguration(nameValueCollection);


            var handler =
                new DefaultAuthorizationHandler(configuration,
                    logger,
                    ImmutableArray<AllowedEmail>.Empty,
                    ImmutableArray<AllowedEmailDomain>.Empty);

            IEnumerable<Claim> claims = new[] { new Claim(CustomClaimTypes.IpAddress, "192.168.1.2") };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var authorizationHandlerContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new DefaultAuthorizationRequirement() },
                user,
                null);

            await handler.HandleAsync(authorizationHandlerContext);

            Assert.False(authorizationHandlerContext.HasSucceeded);
        }

        [Fact]
        public async Task IpClaimWithoutNetworksShouldMarkContextNotSucceeded()
        {
            var logger = Logger.None;

            var handler =
                new DefaultAuthorizationHandler(NoConfiguration.Empty,
                    logger,
                    ImmutableArray<AllowedEmail>.Empty,
                    ImmutableArray<AllowedEmailDomain>.Empty);

            IEnumerable<Claim> claims = new[] { new Claim(CustomClaimTypes.IpAddress, "192.168.1.2") };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var authorizationHandlerContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new DefaultAuthorizationRequirement() },
                user,
                null);

            await handler.HandleAsync(authorizationHandlerContext);

            Assert.False(authorizationHandlerContext.HasSucceeded);
        }
    }
}
