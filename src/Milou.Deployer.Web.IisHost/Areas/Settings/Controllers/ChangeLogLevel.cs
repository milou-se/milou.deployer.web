namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class ChangeLogLevel
    {
        public ChangeLogLevel(string newLevel)
        {
            NewLevel = newLevel;
        }

        public string NewLevel { get; }
    }
}
