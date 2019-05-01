using Arbor.KVConfiguration.Core.Metadata;

namespace Milou.Deployer.Web.IisHost.Areas.NuGet
{
    public static class NuGetListConstants
    {
        public const string Configuration = "urn:milou:deployer:web:nuget:list-configuration";

        [Metadata(defaultValue: "7")]
        public const string DefaultListTimeOutInSeconds =
            Configuration + ":default:" + nameof(NuGetListConfiguration.ListTimeOutInSeconds);
    }
}
