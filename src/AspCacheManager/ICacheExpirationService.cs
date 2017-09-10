using System;
using System.Collections.Generic;

namespace Titanosoft.AspCacheManager
{
    public interface ICacheExpirationService
    {
        Dictionary<CompositeCacheKey, DateTime> CacheExpirations { get; set; }
    }
}