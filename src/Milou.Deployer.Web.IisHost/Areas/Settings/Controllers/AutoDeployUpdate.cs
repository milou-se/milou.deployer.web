namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class AutoDeployUpdate
    {
        public AutoDeployUpdate(bool enabled) => Enabled = enabled;

        public bool Enabled { get; }
    }
}