using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;

namespace Milou.Deployer.Web.Core
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