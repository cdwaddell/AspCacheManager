using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Titanosoft.AspCacheManager
{
    public abstract class SourceCacheModule<T> : CacheModule
    {
        private readonly IMemoryCache _memCache;
        protected SourceCacheModule(string baseKey, IDateTime dateTime, IMemoryCache memCache)
            : base(dateTime, baseKey)
        {
            _memCache = memCache;
        }

        internal abstract Task<T> Factory(IDictionary<string, string> cacheKey, CancellationToken token);

        public override async Task Hydrate(IDictionary<string, string> key, CancellationToken token)
        {
            var returnValue = await Factory(key, token);

            var cacheKey = GetKey(key);
            lock (MyLock)
            {
                Set(cacheKey, returnValue);
            }
        }

        public T Get(CompositeCacheKey key)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (_memCache.TryGetValue(key, out T returnValue))
                return returnValue;

            lock (MyLock)
            {
                //double check in case of race to cache
                if (_memCache.TryGetValue(key, out returnValue))
                    return returnValue;

                //otherwise lookup the data
                return Set(key, Factory(key.SubKeys, new CancellationToken()).Result);
            }
        }

        private T Set(CompositeCacheKey key, T cacheObject)
        {
            //randomize cache to imporve rehydration results (don't expire too many at the same time
            var minutes = Random.Next(45, 50);
            var span = TimeSpan.FromMinutes(minutes);

            //soft expire the cache in half this time
            CacheExpiration[key] = DateTime.UtcNow.AddMinutes(minutes / 2d);
            return _memCache.Set(key, cacheObject, span);
        }
    }
}