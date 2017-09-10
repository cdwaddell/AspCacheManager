using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Titanosoft.AspCacheManager
{
    public abstract class CacheModule
    {
        protected static readonly ConcurrentDictionary<Type, object> LockDictionary = new ConcurrentDictionary<Type, object>();
        protected static Random Random = new Random();
        protected readonly IDateTime DateTime;

        private readonly string _baseKey;
        protected readonly object MyLock;
        public readonly List<CompositeCacheKey> LoadedCacheKeys;
        
        protected CacheModule(IDateTime dateTime, string baseKey)
        {
            DateTime = dateTime;
            _baseKey = baseKey;

            LoadedCacheKeys = new List<CompositeCacheKey>();

            MyLock = LockDictionary.GetOrAdd(GetType(), t => new object());
        }

        protected static readonly IDictionary<CompositeCacheKey, DateTime> CacheExpiration
            = new ConcurrentDictionary<CompositeCacheKey, DateTime>();
        
        public bool CheckExpiration(IDictionary<string, string> key = null)
        {
            var cacheKey = GetKey(key);
            return CacheExpiration.ContainsKey(cacheKey) && CacheExpiration[cacheKey] < DateTime.UtcNow;
        }

        public List<CompositeCacheKey> GetExpired()
        {
            return CacheExpiration
                .Where(c => c.Value < DateTime.UtcNow)
                .Select(c => c.Key)
                .ToList();
        }

        protected CompositeCacheKey GetKey(IDictionary<string, string> key = null)
        {
            if(key == null) key = new Dictionary<string, string>();
            return new CompositeCacheKey(_baseKey, key.ToArray());
        }

        public async Task PreProcessKeys(IDictionary<string, string>[] toProcess, CancellationToken token = new CancellationToken())
        {
            foreach (var key in toProcess)
            {
                await Hydrate(key, token);
            }
        }

        public abstract Task Hydrate(IDictionary<string, string> cacheKey, CancellationToken token);
    }
}