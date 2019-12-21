using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using MediatR;

using Milou.Deployer.Web.Core.Settings;
using Milou.Deployer.Web.IisHost.Areas.Settings.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    [UsedImplicitly]
    public class UpdateSettingsHandler : IRequestHandler<UpdateSettings, Unit>
    {
        private readonly IApplicationSettingsStore _settingsStore;

        public UpdateSettingsHandler(IApplicationSettingsStore martenSettingsStore) => _settingsStore = martenSettingsStore;

        public async Task<Unit> Handle(UpdateSettings request, CancellationToken cancellationToken)
        {
            var applicationSettings = await _settingsStore.GetApplicationSettings(cancellationToken);

            if (request.CacheTime.HasValue)
            {
                applicationSettings.CacheTime = request.CacheTime.Value;
            }

            if (request.NexusConfig is { })
            {
                applicationSettings.NexusConfig.HmacKey = request.NexusConfig.HmacKey;
                applicationSettings.NexusConfig.NuGetSource = request.NexusConfig.NuGetSource;
                applicationSettings.NexusConfig.NuGetConfig = request.NexusConfig.NuGetConfig;
            }

            if (request.AutoDeploy?.Enabled != null)
            {
                applicationSettings.AutoDeploy.Enabled = request.AutoDeploy.Enabled;
                applicationSettings.AutoDeploy.PollingEnabled = request.AutoDeploy.PollingEnabled;
            }

            if (request.ApplicationSettingsCacheTimeout.HasValue && request.ApplicationSettingsCacheTimeout.Value.TotalSeconds >= 0.5D)
            {
                applicationSettings.ApplicationSettingsCacheTimeout = request.ApplicationSettingsCacheTimeout.Value;
            }

            if (request.DefaultMetadataTimeout.HasValue && request.DefaultMetadataTimeout.Value.TotalSeconds >= 0.5D)
            {
                applicationSettings.DefaultMetadataRequestTimeout = request.DefaultMetadataTimeout.Value;
            }

            if (request.MetadataCacheTimeout.HasValue && request.MetadataCacheTimeout.Value.TotalSeconds >= 0.5D)
            {
                applicationSettings.MetadataCacheTimeout = request.MetadataCacheTimeout.Value;
            }

            await _settingsStore.Save(applicationSettings);

            return Unit.Value;
        }
    }
}