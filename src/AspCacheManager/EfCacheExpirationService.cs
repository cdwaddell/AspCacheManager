using System;
using System.Collections.Generic;
using System.Linq;
using Titanosoft.AspCacheManager.Data;
using Titanosoft.AspCacheManager.Models;

namespace Titanosoft.AspCacheManager
{
    public class EfCacheExpirationService: ICacheExpirationService
    {
        private readonly ICacheExpirationContext _context;
        private readonly IDateTime _dateTime;

        public EfCacheExpirationService(ICacheExpirationContext context, IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }
        
        public List<CacheExpiration> GetExpirationsSince(DateTime utcDateTime)
        {
            return _context.ExpiredCacheKeys
                .Where(x => x.UtcExpiration > utcDateTime && x.UtcExpiration <= _dateTime.UtcNow)
                .AsEnumerable()
                .Select(x => x.ToModel())
                .ToList();
        }

        public void ExpireCache(CompositeCacheKey key)
        {
            ExpireCache(key, _dateTime.UtcNow);
        }

        public void ExpireCache(CompositeCacheKey key, DateTime expiration)
        {
            ExpireCache(new CacheExpiration{CacheKey = key, Expiration = expiration });
        }

        private void ExpireCache(CacheExpiration key)
        {
            _context.ExpiredCacheKeys
                .Add(key.ToEntity());

            _context.SaveChanges();
        }
    }
}