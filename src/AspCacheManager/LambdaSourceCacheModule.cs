using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Titanosoft.AspCacheManager
{
    public sealed class LambdaSourceCacheModule<T> : SourceCacheModule<T> where T: class
    {
        public static readonly Func<IDictionary<string, string>, CancellationToken,Task<T>> LambdaFactory = null;

        public LambdaSourceCacheModule(string baseKey, IDateTime dateTime, IMemoryCache memCache) 
            : base(baseKey, dateTime, memCache)
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