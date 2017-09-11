using Microsoft.Extensions.DependencyInjection;

namespace Titanosoft.AspCacheManager
{
    public interface ICacheManagerBuilder
    {
        IServiceCollection Services { get; set; }
    }
}