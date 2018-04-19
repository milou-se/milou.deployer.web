using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    [UsedImplicitly]
    public class MilouAuthenticationHandler : AuthenticationHandler<MilouAuthenticationOptions>
    {
        public MilouAuthenticationHandler(
            IOptionsMonitor<MilouAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string address = Context.Connection.RemoteIpAddress?.ToString();

            var claims = new List<Claim>();
            AuthenticateResult authenticateResult;
            if (!string.IsNullOrWhiteSpace(address))
            {
                claims.Add(new Claim(CustomClaimTypes.IPAddress, address));

                authenticateResult = AuthenticateResult.Success(
                    new AuthenticationTicket(
                        new ClaimsPrincipal(new ClaimsIdentity(claims)),
                        new AuthenticationProperties(),
                        Scheme.Name));
            }
            else
            {
                authenticateResult = AuthenticateResult.Fail("Missing remote ip address");
            }

            return Task.FromResult(
                authenticateResult);
        }
    }
}