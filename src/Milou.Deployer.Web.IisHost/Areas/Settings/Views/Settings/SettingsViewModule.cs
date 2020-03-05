using System.Collections.Immutable;
using System.Linq;
using Milou.Deployer.Web.Core;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Views.Settings
{
    public sealed class SettingsViewModule
    {
        public static readonly SettingsViewModule LogLevel = new SettingsViewModule(nameof(LogLevel), 1);
        public static readonly SettingsViewModule AppVersion = new SettingsViewModule(nameof(AppVersion), 5);
        public static readonly SettingsViewModule AppMetadata = new SettingsViewModule(nameof(AppMetadata), 3);
        public static readonly SettingsViewModule AppSettings = new SettingsViewModule(nameof(AppSettings), 4);
        public static readonly SettingsViewModule IpInfo = new SettingsViewModule(nameof(IpInfo), 6);
        public static readonly SettingsViewModule PackageCache = new SettingsViewModule(nameof(PackageCache), 2);
        public static readonly SettingsViewModule Routes = new SettingsViewModule(nameof(Routes), 7);

        public static readonly SettingsViewModule
            AppConfiguration = new SettingsViewModule(nameof(AppConfiguration), 8);

        public SettingsViewModule(string invariantName, int order)
        {
            InvariantName = invariantName;
            Order = order;
        }

        public static ImmutableArray<SettingsViewModule> All => EnumerableOf<SettingsViewModule>.All.OrderBy(module => module.Order).ToImmutableArray();

        public string InvariantName { get; }

        public int Order { get; }
    }
}