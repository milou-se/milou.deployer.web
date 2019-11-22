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
                applicationSettings.NexusConfig.HmacKey = request.NexusConfig.NuGetSource;
                applicationSettings.NexusConfig.HmacKey = request.NexusConfig.NuGetConfig;
            }

            await _settingsStore.Save(applicationSettings);

            return Unit.Value;
        }
    }
}