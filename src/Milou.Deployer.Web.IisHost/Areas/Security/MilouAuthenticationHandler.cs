using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Milou.Deployer.Web.Core.Security;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [UsedImplicitly]
    public class MilouAuthenticationHandler : AuthenticationHandler<MilouAuthenticationOptions>
    {
        private readonly ILogger _logger;

        public MilouAuthenticationHandler(
            [NotNull] ILogger logger,
            IOptionsMonitor<MilouAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, loggerFactory, encoder, clock)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
                var address = Context.Connection.RemoteIpAddress?.ToString();
                _logger.Verbose(
                    "User ip from address {Address} is forbidden, challenge not supported",
                    address);
            }

            return base.HandleForbiddenAsync(properties);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
                var address = Context.Connection.RemoteIpAddress?.ToString();
                _logger.Verbose(
                    "Could not authenticate current user ip from address {Address}, challenge not supported",
                    address);
            }

            return base.HandleChallengeAsync(properties);
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var address = Context.Connection.RemoteIpAddress?.ToString();

            var claims = new List<Claim>();
            AuthenticateResult authenticateResult;
            if (!string.IsNullOrWhiteSpace(address))
            {
                claims.Add(new Claim(CustomClaimTypes.IpAddress, address));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, address));

                authenticateResult = AuthenticateResult.Success(
                    new AuthenticationTicket(
                        new ClaimsPrincipal(new ClaimsIdentity(claims)),
                        new AuthenticationProperties(),
                        Scheme.Name));

                if (_logger.IsEnabled(LogEventLevel.Verbose))

                {
                    _logger.Verbose("Settings current user ip claim to {Address}", address);
                }
            }
            else
            {
                if (_logger.IsEnabled(LogEventLevel.Verbose))
                {
                    _logger.Verbose("Could not set current user ip claim to any address");
                }

                authenticateResult = AuthenticateResult.Fail("Missing remote ip address");
            }

            return Task.FromResult(authenticateResult);
        }
    }
}
