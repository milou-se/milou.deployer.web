using Arbor.KVConfiguration.Core.Metadata;
using Arbor.KVConfiguration.Urns;

namespace Milou.Deployer.Web.Core.NuGet
{
    [Urn(Urn)]
    [Optional]
    public class NuGetConfiguration
    {
        public const string Urn = "urn:milou:deployer:web:nuget-configuration";

        [Metadata]
        public const string Default = "urn:milou:deployer:web:nuget-configuration:default:nuget-exe-path";

        public NuGetConfiguration(string? nugetExePath = null) => NugetExePath = nugetExePath;

        public string? NugetExePath { get; set; }
    }
}