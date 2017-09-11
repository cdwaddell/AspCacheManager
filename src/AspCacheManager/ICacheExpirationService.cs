using System;
using System.Collections.Generic;
using Titanosoft.AspCacheManager.Models;

namespace Titanosoft.AspCacheManager
{
    public interface ICacheExpirationService
    {
        List<CacheExpiration> GetExpirationsSince(DateTime utcDateTime);
        void ExpireCache(CompositeCacheKey key);
        void ExpireCache(CompositeCacheKey key, DateTime expiration);
    }
}