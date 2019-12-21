namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class AutoDeployUpdate
    {
        public AutoDeployUpdate(bool enabled, bool pollingEnabled)
        {
            Enabled = enabled;
            PollingEnabled = pollingEnabled;
        }

        public bool Enabled { get; }

        public bool PollingEnabled { get; }
    }
}