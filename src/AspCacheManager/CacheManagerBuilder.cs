using Microsoft.Extensions.DependencyInjection;

namespace Titanosoft.AspCacheManager
{
    public class CacheManagerBuilder: ICacheManagerBuilder
    {
        public IServiceCollection Services { get; set; }
    }
}