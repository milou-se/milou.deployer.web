using Arbor.KVConfiguration.Core.Metadata;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public static class WorkerConstants
    {
        public const string Configuration = "urn:milou:deployer:web:deployment-worker:configuration";

        [Metadata(defaultValue: "10")]
        public const string MessageTimeOutInSeconds = Configuration + ":default:" + nameof(MessageTimeOutInSeconds);
    }
}