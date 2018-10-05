using System.Threading;
using System.Threading.Tasks;

namespace Milou.Deployer.Web.Core
{
    public interface IDataSeeder
    {
        Task SeedAsync(CancellationToken cancellationToken);
    }
}