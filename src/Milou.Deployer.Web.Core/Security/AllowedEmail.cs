using Arbor.KVConfiguration.Urns;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.Core.Security
{
    [Optional]
    [Urn(UrnKey)]
    public class AllowedEmail : IConfigurationValues, IValidationObject
    {
        public const string UrnKey = "urn:milou:deployer:web:allowed-email";

        public AllowedEmail(string email)
        {
            Email = email;
            IsValid = EmailAddress.TryParse(email, out _);
        }

        public string Email { get; }

        public bool IsValid { get; }
    }
}
