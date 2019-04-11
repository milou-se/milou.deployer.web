using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Configuration
{
    [Optional]
    [Urn(RegistrationConstants.ExcludedType)]
    [UsedImplicitly]
    public class ExcludedAutoRegistrationType
    {
        public ExcludedAutoRegistrationType(string fullName)
        {
            FullName = fullName;
        }

        public string FullName { get; }
    }
}
