using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Milou.Deployer.Web.Core.Caching;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Marten
{
    internal static class EnvironmentTypeDataExtensions
    {
        private const string CacheKey = "EnvironmentTypes"; // TODO improve key

        public static async Task<ImmutableArray<EnvironmentType>> GetEnvironmentTypes(this IDocumentStore documentStore, ICustomMemoryCache memoryCache, CancellationToken cancellationToken =
            default)
        {
            if (memoryCache.TryGetValue(CacheKey, out EnvironmentType[] environmentTypes))
            {
                return environmentTypes.ToImmutableArray();
            }

            using var querySession = documentStore.QuerySession();

            var environmentTypeData = await querySession.Query<EnvironmentTypeData>()
                .ToListAsync<EnvironmentTypeData>(cancellationToken);

            var enumerable = environmentTypeData.Select(EnvironmentTypeData.MapFromData).ToArray();

            memoryCache.SetValue(CacheKey, enumerable);

            return enumerable.ToImmutableArray();
        }
    }
}