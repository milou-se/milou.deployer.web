using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Arbor.App.Extensions.Configuration
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
