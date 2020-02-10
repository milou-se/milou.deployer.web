using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Core
{
    public interface IEnvironmentTypeService
    {
        Task<ImmutableArray<EnvironmentType>> GetEnvironmentTypes(CancellationToken cancellationToken = default);
    }
}