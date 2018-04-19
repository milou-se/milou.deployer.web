using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core.Security
{
    public class SimpleAuthenticator
    {
        public Task<IEnumerable<Claim>> IsAuthenticated(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Task.FromResult<IEnumerable<Claim>>(new List<Claim>());
            }

            string storedUsername =
                StaticKeyValueConfigurationManager.AppSettings[ConfigurationConstants.UsernameKey].ThrowIfEmpty(
                    $"AppSetting key '{ConfigurationConstants.UsernameKey}' is not set");
            string storedPassword =
                StaticKeyValueConfigurationManager.AppSettings[ConfigurationConstants.PasswordKey].ThrowIfEmpty(
                    $"AppSetting key '{ConfigurationConstants.PasswordKey}' is not set");

            bool correctUsername = username.Equals(storedUsername, StringComparison.InvariantCultureIgnoreCase);
            bool correctPassword = password.Equals(storedPassword, StringComparison.InvariantCulture);

            if (!(correctUsername && correctPassword))
            {
                return Task.FromResult<IEnumerable<Claim>>(new List<Claim>());
            }

            return
                Task.FromResult<IEnumerable<Claim>>(
                    new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, storedUsername),
                        new Claim(ClaimTypes.NameIdentifier, storedUsername)
                    });
        }
    }
}