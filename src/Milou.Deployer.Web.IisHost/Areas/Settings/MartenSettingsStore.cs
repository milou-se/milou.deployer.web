using System;
using System.Threading;
using System.Threading.Tasks;

using Marten;

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

        private ApplicationSettings Map(ApplicationSettingsData applicationSettingsData) =>
            new ApplicationSettings { CacheTime = applicationSettingsData?.CacheTime ?? TimeSpan.FromSeconds(300) };

        private ApplicationSettingsData MapToData(ApplicationSettings applicationSettings) =>
            new ApplicationSettingsData { CacheTime = applicationSettings.CacheTime, Id = AppSettings };
    }
}