namespace Milou.Deployer.Web.Core.Logging
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
