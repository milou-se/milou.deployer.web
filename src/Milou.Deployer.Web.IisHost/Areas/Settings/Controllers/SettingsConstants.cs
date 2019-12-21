namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public static class SettingsConstants
    {
        public const string DiagnosticsEnabled = "urn:milou:deployer:web:diagnostics:enabled";

        public const string AreaName = "Settings";

        public const string SettingsGetRoute = "/settings";

        public const string SettingsGetRouteName = nameof(SettingsGetRoute);

        public const string LogSettingsPostRoute = "/settings/loglevel";

        public const string LogSettingsPostRouteName = nameof(LogSettingsPostRoute);

        public const string SaveSettingsPostRoute = "/settings/application";

        public const string SaveSettingsPostRouteName = nameof(SaveSettingsPostRoute);
    }
}
