namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class ConfigurationKeyInfo
    {
        public string Key { get; }
        public string Value { get; }
        public string Source { get; }

        public ConfigurationKeyInfo(string key, string value, string source)
        {
            Key = key;
            Value = value;
            Source = source;
        }
    }
}