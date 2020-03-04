﻿using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Caching;
using Milou.Deployer.Web.Core.Deployment;

namespace Milou.Deployer.Web.Marten.EnvironmentTypes
{
    public class EnvironmentTypeService : IEnvironmentTypeService
    {
        private readonly ICustomMemoryCache _cache;

        private readonly IDocumentStore _store;

        public EnvironmentTypeService(IDocumentStore martenStore, ICustomMemoryCache cache)
        {
            _store = martenStore;
            _cache = cache;
        }

        public Task<ImmutableArray<EnvironmentType>> GetEnvironmentTypes(CancellationToken cancellationToken =
            default) =>
            _store.GetEnvironmentTypes(_cache, cancellationToken);
    }
}