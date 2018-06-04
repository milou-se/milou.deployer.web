using System.Collections.Immutable;

namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    public class SettingsViewModel
    {
        public string TargetReadService { get; }

        public ImmutableArray<(string Type, string Name, string Value)> Routes { get; }

        public SettingsViewModel(string targetReadService, ImmutableArray<(string, string, string)> routes)
        {
            TargetReadService = targetReadService;
            Routes = routes;
        }
    }
}