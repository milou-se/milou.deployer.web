using Arbor.KVConfiguration.Urns;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.Core.Security
{
    [Optional]
    [Urn(UrnKey)]
    public class AllowedEmailDomain : IConfigurationValues, IValidationObject
    {
        public const string UrnKey = "urn:milou:deployer:web:allowed-email-domain";

        public AllowedEmailDomain(string domain)
        {
            Domain = domain;
            IsValid = !string.IsNullOrWhiteSpace(domain);
        }

        public string Domain { get; }

        public bool IsValid { get; }
    }
}
