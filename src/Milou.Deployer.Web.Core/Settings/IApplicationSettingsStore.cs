using System.Threading;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Core.Settings
{
    public interface IApplicationSettingsStore
    {
        Task<ApplicationSettings> GetApplicationSettings(CancellationToken cancellationToken);

        Task Save(ApplicationSettings applicationSettings);
    }
}