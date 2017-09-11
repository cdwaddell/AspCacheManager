using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Titanosoft.AspCacheManager.Data;
using Titanosoft.AspCacheManager.Models;

namespace Titanosoft.AspCacheManager
{
    public class EfCacheExpirationServiceOptions
    {
        public bool EnableMigrations { get; set; }
    }

    public class EfCacheExpirationService: ICacheExpirationService
    {
        private static EfCacheExpirationServiceOptions _options;
        private readonly ICacheExpirationContext _context;
        private readonly IDateTime _dateTime;

        public EfCacheExpirationService(ICacheExpirationContext context, IDateTime dateTime, EfCacheExpirationServiceOptions options)
        {
            _context = context;
            _dateTime = dateTime;

            if (_options != null)
                return;

            _options = options;
            if (options.EnableMigrations)
                context.Database.Migrate();
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