using Arbor.KVConfiguration.Core.Metadata;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    public static class MonitorConstants
    {
        public const string MonitorRoute = "~/";

        public const string MonitorRouteName = nameof(MonitorRoute);

        public const string MonitorConfiguration = "urn:milou:deployer:web:monitor-configuration";

        [Metadata(defaultValue: "10")]
        public const string MonitorConfigurationDefaultTimeoutInSeconds =
            "urn:milou:deployer:web:monitor-configuration:default:default-timeout-in-seconds";
    }
}
