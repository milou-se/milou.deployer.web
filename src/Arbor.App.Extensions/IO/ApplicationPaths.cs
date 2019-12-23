using Arbor.KVConfiguration.Urns;

namespace Milou.Deployer.Web.Core.IO
{
    [Urn(Urn)]
    [Optional]
    public class ApplicationPaths
    {
        public const string Urn = "urn:milou:deployer:web:paths";

        public string BasePath { get; set; }

        public string ContentBasePath { get; set; }
    }
}
