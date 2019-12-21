using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Milou.Deployer.Web.Core.Caching;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Caching
{
    [UsedImplicitly]
    public class CustomMemoryCache : ICustomMemoryCache
    {
        private static readonly ConcurrentDictionary<string, object> Keys =
            new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;

        public CustomMemoryCache(IMemoryCache memoryCache, ILogger logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        private IReadOnlyCollection<string> GetCachedKeys()
        {
            (string key, bool exists)[] keys = Keys.Select(key => (key.Key, _memoryCache.TryGetValue(key.Key, out _)))
                .ToArray();

            var toRemove = keys.Where(item => !item.exists).Select(item => item.key).ToArray();

            foreach (var nonCachedKey in toRemove)
            {
                var removed = Keys.TryRemove(nonCachedKey, out _);

                if (!removed)
                {
                    _logger.Debug("Could not remove cached item with key {CacheKey}", nonCachedKey);
                }
            }

            return Keys.Keys.ToArray();
        }

        public IReadOnlyCollection<string> CachedKeys => GetCachedKeys();

        public bool TryGetValue<T>(string key, out T item) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }

            if (_memoryCache.TryGetValue(key, out var cachedItem) && cachedItem is T cachedItemOfT)
            {
                item = cachedItemOfT;
                return true;
            }

            item = default;
            return false;
        }

        public void SetValue<T>(string key, T item, TimeSpan? cacheTime) where T : class
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }


            var cacheEntryAbsoluteExpirationRelativeToNow =
                cacheTime?.TotalSeconds > 0
                    ? cacheTime.Value
                    : TimeSpan.FromSeconds(900);

            _memoryCache.Set(key, item, cacheEntryAbsoluteExpirationRelativeToNow);
            bool added = Keys.TryAdd(key, string.Empty);

            if (!added && !Keys.ContainsKey(key))
            {
                _logger.Debug("Could not add item with key {Key} to cache", key);
            }
        }

        public void Invalidate(string prefix = null)
        {
            var keys = GetCachedKeys();

            if (!keys.Any())
            {
                _logger.Debug("No items were removed from in-memory cache since there were no cached items");
                return;
            }

            var filteredKeys = keys;

            if (prefix.HasValue())
            {
                filteredKeys = keys
                    .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }

            _logger.Debug(
                "Removing {ToRemoveCount} items of {TotalCount} from in-memory cache matching prefix {Prefix}",
                filteredKeys.Count,
                keys.Count,
                prefix);

            foreach (var key in filteredKeys)
            {
                _memoryCache.Remove(key);
                Keys.TryRemove(key, out _);
                _logger.Debug("Removed item with key {CacheKey} from in-memory cache", key);
            }
        }
    }
}
