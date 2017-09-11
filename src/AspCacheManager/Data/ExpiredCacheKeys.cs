using System;

namespace Titanosoft.AspCacheManager.Data
{
    public class ExpiredCacheKey
    {
        public int Id { get; set; }

        public string JsonCacheKey { get; set; }

        public DateTime UtcExpiration { get; set; }
    }
}
