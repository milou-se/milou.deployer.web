using System;
using Microsoft.AspNetCore.Authentication;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    public static class CustomAuthenticationExtensions
    {
        public static AuthenticationBuilder AddMilouAuthentication(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            string displayName,
            Action<MilouAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<MilouAuthenticationOptions, MilouAuthenticationHandler>(authenticationScheme,
                displayName,
                configureOptions);
        }
    }
}
