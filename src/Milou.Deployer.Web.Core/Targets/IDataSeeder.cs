using System.Threading;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Core.Targets
{
    public interface IDataSeeder
    {
        Task SeedAsync(CancellationToken cancellationToken);
    }
}
