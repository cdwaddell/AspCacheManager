using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Titanosoft.AspCacheManager
{
    public sealed class LambdaCacheModule<T> : SourceCacheModule<T> where T: class
    {
        public static Func<IDictionary<string, string>, CancellationToken, Task<T>> LambdaFactory = null;
        public static string BaseKey = null;
        
        public LambdaCacheModule(IDateTime dateTime, IMemoryCache memCache) 
            : base(BaseKey, dateTime, memCache)
        {
        }

        internal override Task<T> Factory(IDictionary<string, string> cacheKey, CancellationToken token)
        {
            return LambdaFactory == null ? 
                Task.FromResult((T) null) : 
                LambdaFactory(cacheKey, token);
        }
    }
}