using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [UsedImplicitly]
    public class CustomMemoryCache : ICustomMemoryCache
    {
        private static readonly ConcurrentDictionary<string, object> _Keys = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public CustomMemoryCache(IMemoryCache memoryCache, ILogger logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public IReadOnlyCollection<string> CachedKeys => GetCachedKeys();

        public bool TryGetValue<T>(string key, out T item) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }

            if (_memoryCache.TryGetValue(key, out object cachedItem) && cachedItem is T cachedItemOfT)
            {
                item = cachedItemOfT;
                return true;
            }

            item = default;
            return false;
        }

        public void Set<T>(string key, T item) where T : class
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }

            TimeSpan cacheEntryAbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(900);

            _memoryCache.Set(key, item, cacheEntryAbsoluteExpirationRelativeToNow);
            bool added = _Keys.TryAdd(key, string.Empty);

            if (!added && !_Keys.ContainsKey(key))
            {
                _logger.Debug("Could not add item with key {Key} to cache", key);
            }
        }

        public void Invalidate(string prefix = null)
        {
            IReadOnlyCollection<string> keys = GetCachedKeys();

            if (!keys.Any())
            {
                _logger.Debug("No items were removed from in-memory cache since there were no cached items");
                return;
            }

            IReadOnlyCollection<string> filteredKeys = keys;

            if (prefix.HasValue())
            {
                filteredKeys = keys
                    .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }

            _logger.Debug("Removing {ToRemoveCount} items of {TotalCount} from in-memory cache matching prefix {Prefix}", filteredKeys.Count, keys.Count, prefix);

            foreach (string key in filteredKeys)
            {
                _memoryCache.Remove(key);
                _Keys.TryRemove(key, out _);
                _logger.Debug("Removed item with key {CacheKey} from in-memory cache", key);
            }
        }

        private IReadOnlyCollection<string> GetCachedKeys()
        {
            (string key, bool exists)[] keys = _Keys.Select(key => (key.Key, _memoryCache.TryGetValue(key.Key, out _)))
                .ToArray();

            string[] toRemove = keys.Where(item => !item.exists).Select(item => item.key).ToArray();

            foreach (string nonCachedKey in toRemove)
            {
                bool removed = _Keys.TryRemove(nonCachedKey, out _);

                if (!removed)
                {
                    _logger.Debug("Could not remove cached item with key {CacheKey}", nonCachedKey);
                }
            }

            return _Keys.Keys.ToArray();
        }
    }
}