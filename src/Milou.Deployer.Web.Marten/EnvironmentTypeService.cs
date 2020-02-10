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

        public Task<ImmutableArray<EnvironmentType>> GetEnvironmentTypes(CancellationToken cancellationToken =
            default) =>
            _store.GetEnvironmentTypes(cancellationToken);
    }


    internal static class EnvironmentTypeDataExtensions
    {
        public static async Task<ImmutableArray<EnvironmentType>> GetEnvironmentTypes(this IDocumentStore documentStore, CancellationToken cancellationToken =
            default)
        {
            using var querySession = documentStore.QuerySession();

            var environmentTypeData = await querySession.Query<EnvironmentTypeData>()
                .ToListAsync<EnvironmentTypeData>(cancellationToken);

            return environmentTypeData.Select(EnvironmentTypeData.MapFromData).ToImmutableArray();
        }
    }
}