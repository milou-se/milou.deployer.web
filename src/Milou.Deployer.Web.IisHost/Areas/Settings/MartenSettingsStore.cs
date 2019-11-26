using System;
using System.Threading;
using System.Threading.Tasks;

using Marten;

using Milou.Deployer.Web.Core.Integration.Nexus;
using Milou.Deployer.Web.Core.Settings;

namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    public class MartenSettingsStore : IApplicationSettingsStore
    {
        private const string AppSettings = "appsettings";

        private readonly IDocumentStore _documentStore;

        public MartenSettingsStore(IDocumentStore documentStore) => _documentStore = documentStore;

        public async Task<ApplicationSettings> GetApplicationSettings(CancellationToken cancellationToken)
        {
            ApplicationSettings applicationSettings;
            using (var querySession = _documentStore.QuerySession())
            {
                var applicationSettingsData =
                    await querySession.LoadAsync<ApplicationSettingsData>(AppSettings, cancellationToken);

                applicationSettings = Map(applicationSettingsData);
            }

            return applicationSettings;
        }

        public async Task Save(ApplicationSettings applicationSettings)
        {
            using (var querySession = _documentStore.OpenSession())
            {
                ApplicationSettingsData data = MapToData(applicationSettings);
                querySession.Store(data);

                await querySession.SaveChangesAsync();
            }
        }

        private ApplicationSettings Map(ApplicationSettingsData applicationSettingsData)
        {
            var applicationSettings = new ApplicationSettings
                                      {
                                          CacheTime = applicationSettingsData?.CacheTime ?? TimeSpan.FromSeconds(300),
                                          NexusConfig = MapFromNexusData(applicationSettingsData?.NexusConfig),
                                          AutoDeploy = MapAutoDeploy(applicationSettingsData?.AutoDeploy)
                                      };

            return applicationSettings;
        }

        private AutoDeploySettings MapAutoDeploy(AutoDeployData autoDeploy) =>
            new AutoDeploySettings { Enabled = autoDeploy?.Enabled ?? false };

        private NexusConfig MapFromNexusData(NexusConfigData data) =>
            new NexusConfig
            {
                HmacKey = data?.HmacKey, NuGetSource = data?.NuGetSource, NuGetConfig = data?.NuGetConfig
            };

        private AutoDeployData MapToAutoDeployData(AutoDeploySettings autoDeploySettings) =>
            new AutoDeployData { Enabled = autoDeploySettings?.Enabled ?? false };

        private ApplicationSettingsData MapToData(ApplicationSettings applicationSettings) =>
            new ApplicationSettingsData
            {
                CacheTime = applicationSettings.CacheTime,
                Id = AppSettings,
                NexusConfig = MapToNexusData(applicationSettings.NexusConfig),
                AutoDeploy = MapToAutoDeployData(applicationSettings.AutoDeploy)
            };

        private NexusConfigData MapToNexusData(NexusConfig nexusConfig) =>
            new NexusConfigData
            {
                HmacKey = nexusConfig?.HmacKey,
                NuGetSource = nexusConfig?.NuGetSource,
                NuGetConfig = nexusConfig?.NuGetConfig
            };
    }
}