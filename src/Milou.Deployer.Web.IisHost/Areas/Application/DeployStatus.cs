namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public sealed class DeployStatus
    {
        public static readonly DeployStatus Latest = new DeployStatus("latest", "Latest");
        public static readonly DeployStatus NoPackagesAvailable = new DeployStatus("no-packages", "No packages available");
        public static readonly DeployStatus UpdateAvailable = new DeployStatus("update-available", "Update available");
        public static readonly DeployStatus Unavailable = new DeployStatus("unavailable", "Unavailable");

        private DeployStatus(string key, string displayName)
        {
            Key = key;
            DisplayName = displayName;
        }

        public string Key { get; }

        public string DisplayName { get; }
    }
}