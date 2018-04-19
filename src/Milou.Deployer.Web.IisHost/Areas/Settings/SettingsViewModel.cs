namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    public class SettingsViewModel
    {
        public string TargetReadService { get; }

        public SettingsViewModel(string targetReadService)
        {
            TargetReadService = targetReadService;
        }
    }
}