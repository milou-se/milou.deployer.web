using Arbor.KVConfiguration.Urns;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    [Urn(RegistrationConstants.ExcludedType)]
    public class ExcludedAutoRegistrationType
    {
        public string FullName { get; }

        public ExcludedAutoRegistrationType(string fullName)
        {
            FullName = fullName;
        }
    }
}