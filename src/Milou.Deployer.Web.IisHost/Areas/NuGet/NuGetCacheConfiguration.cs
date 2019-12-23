﻿using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.NuGet
{
    [PublicAPI]
    [Urn(NuGetCacheConstants.Configuration)]
    [UsedImplicitly]
    public class NuGetCacheConfiguration : IConfigurationValues
    {
        private const int DefaultCacheRefreshIntervalInSeconds = 180;

        private const int DefaultCacheRefreshStartupDelayInSeconds = 120;

        public NuGetCacheConfiguration(
            int cacheRefreshIntervalInSeconds = DefaultCacheRefreshIntervalInSeconds,
            int startupDelayInSeconds = DefaultCacheRefreshStartupDelayInSeconds)
        {
            CacheRefreshIntervalInSeconds = cacheRefreshIntervalInSeconds <= 0
                ? DefaultCacheRefreshIntervalInSeconds
                : cacheRefreshIntervalInSeconds;
            StartupDelayInSeconds = startupDelayInSeconds <= 0
                ? DefaultCacheRefreshStartupDelayInSeconds
                : startupDelayInSeconds;
        }

        public int CacheRefreshIntervalInSeconds { get; }

        public int StartupDelayInSeconds { get; }
    }
}
