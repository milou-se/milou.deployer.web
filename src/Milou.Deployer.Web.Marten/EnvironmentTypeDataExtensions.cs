using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Marten
{
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