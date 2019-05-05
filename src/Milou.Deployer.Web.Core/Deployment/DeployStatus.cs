namespace Milou.Deployer.Web.Core.Deployment
{
    public sealed class DeployStatus
    {
        public static readonly DeployStatus Latest = new DeployStatus("latest", "Latest");

        public static readonly DeployStatus NoPackagesAvailable =
            new DeployStatus("no-packages", "No packages available");

        public static readonly DeployStatus UpdateAvailable = new DeployStatus("update-available", "Update available");

        public static readonly DeployStatus Unavailable = new DeployStatus("unavailable", "Unavailable");

        public static readonly DeployStatus Unknown = new DeployStatus("unknown", "unknown");

        public static readonly DeployStatus NoLaterAvailable =
            new DeployStatus("no-later-available", "No later version available");

        private DeployStatus(string key, string displayName)
        {
            Key = key;
            DisplayName = displayName;
        }

        public string Key { get; }

        public string DisplayName { get; }
    }
}
