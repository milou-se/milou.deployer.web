using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Marten
{
    public class EnvironmentTypeService : IEnvironmentTypeService
    {
        private readonly IDocumentStore _store;

        public EnvironmentTypeService(IDocumentStore martenStore) => _store = martenStore;

        public async Task<ImmutableArray<EnvironmentType>> GetEnvironmentTypes(CancellationToken cancellationToken =
            default)
        {
            using var querySession = _store.QuerySession();

            var environmentTypeData = await querySession.Query<EnvironmentTypeData>()
                .ToListAsync<EnvironmentTypeData>(cancellationToken);

            return environmentTypeData.Select(EnvironmentTypeData.MapFromData).ToImmutableArray();
        }
    }
}