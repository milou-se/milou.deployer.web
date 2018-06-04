using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Services
{
    [UsedImplicitly]
    public class CustomMemoryCache : ICustomMemoryCache
    {
        private readonly ConcurrentDictionary<string, object> _keys = new ConcurrentDictionary<string, object>();
        private readonly IMemoryCache _memoryCache;

        public CustomMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
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

            TimeSpan cacheEntryAbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(120);

            _memoryCache.Set(key, item, cacheEntryAbsoluteExpirationRelativeToNow);
            _keys.TryAdd(key, string.Empty);
        }

        public void Invalidate(string prefix = null)
        {
            IReadOnlyCollection<string> keys = GetCachedKeys();

            if (prefix.HasValue())
            {
                keys = keys
                    .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }

            foreach (string key in keys)
            {
                _memoryCache.Remove(key);
                _keys.TryRemove(key, out _);
            }
        }

        private IReadOnlyCollection<string> GetCachedKeys()
        {
            (string key, bool exists)[] keys = _keys.Select(key => (key.Key, _memoryCache.TryGetValue(key, out _)))
                .ToArray();

            string[] toRemove = keys.Where(item => !item.exists).Select(item => item.key).ToArray();

            foreach (string nonCachedKey in toRemove)
            {
                _keys.TryRemove(nonCachedKey, out _);
            }

            return _keys.Keys.ToArray();
        }
    }
}