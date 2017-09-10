using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Titanosoft.AspCacheManager
{
    public sealed class CacheManager:ICacheManager
    {
        private static DateTime? _lastRun;

        private readonly IEnumerable<CacheModule> _cacheModules;
        private readonly IDateTime _dateTime;
        private readonly ILogger<CacheManager> _logger;
        private readonly ICacheExpirationService _cacheService;

        public CacheManager(IEnumerable<CacheModule> cacheModules, ICacheExpirationService cacheService, IDateTime dateTime, ILogger<CacheManager> logger)
        {
            _logger = logger;
            _cacheModules = cacheModules;
            _dateTime = dateTime;
            _cacheService = cacheService;
        }

        public async Task CheckRefreshAsync(CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                    return;

                List<CompositeCacheKey> refreshKeys = null;

                var utcNow = _dateTime.UtcNow;

                if (_lastRun != null)
                    refreshKeys = _cacheService.CacheExpirations
                        .Where(c => _lastRun <= c.Value && c.Value <= utcNow)
                        .Select(c => c.Key).ToList();

                _lastRun = utcNow;
                
                //refresh all soft expired cache
                foreach (var module in _cacheModules)
                foreach (var subModule in module.LoadedCacheKeys)
                {
                    if (token.IsCancellationRequested)
                        return;
                    try
                    {
                        //if we have never run this before, or somone flagged this as expired, or if it expired naturally
                        if (refreshKeys == null || refreshKeys.Contains(subModule) || module.CheckExpiration(subModule.SubKeys))
                        {
                            await module.Hydrate(subModule.SubKeys, token);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(new EventId(348), ex, $"Job Failure for {module}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(new EventId(348), ex, "Background job is unable to run");
            }
        }
    }
}
