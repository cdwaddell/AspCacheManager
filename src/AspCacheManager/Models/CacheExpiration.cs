using System;

namespace Titanosoft.AspCacheManager.Models
{
    public class CacheExpiration
    {
        public CompositeCacheKey CacheKey { get; set; }

        public DateTime Expiration { get; set; }
    }
}