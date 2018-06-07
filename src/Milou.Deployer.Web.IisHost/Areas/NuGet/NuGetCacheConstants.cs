using Arbor.KVConfiguration.Core.Metadata;

namespace Milou.Deployer.Web.IisHost.Areas.NuGet
{
    public static class NuGetCacheConstants
    {
        public const string Configuration = "urn:milou:deployer:web:nuget:cache:configuration";

        [Metadata(defaultValue: "true")]
        public const string Enabled = "urn:milou:deployer:web:nuget:cache:configuration:default:enabled";
    }
}