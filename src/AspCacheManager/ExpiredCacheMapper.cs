using Newtonsoft.Json;
using Titanosoft.AspCacheManager.Data;
using Titanosoft.AspCacheManager.Models;

namespace Titanosoft.AspCacheManager
{
    public static class ExpiredCacheMapper
    {
        public static CacheExpiration ToModel(this ExpiredCacheKey key)
        {
            return key == null ? null : 
                new CacheExpiration
                {
                    Expiration = key.UtcExpiration.ToLocalTime(),
                    CacheKey = JsonConvert.DeserializeObject<CompositeCacheKey>(key.JsonCacheKey)
                };
        }

        public static ExpiredCacheKey ToEntity(this CacheExpiration model)
        {
            return model == null? null:
                new ExpiredCacheKey
                {
                    JsonCacheKey = JsonConvert.SerializeObject(model.CacheKey),
                    UtcExpiration = model.Expiration.ToUniversalTime()
                };
        }
    }
}
