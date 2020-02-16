using System.Threading;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Core.Deployment.Targets
{
    public interface IDataSeeder
    {
        Task SeedAsync(CancellationToken cancellationToken);

        int Order { get; }
    }
}
