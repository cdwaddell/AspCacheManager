using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Titanosoft.AspBackgroundWorker;

namespace Titanosoft.AspCacheManager
{
    public interface ICacheManager
    {
        Task CheckRefreshAsync(CancellationToken token);
    }

    public static class ServiceExtensions
    {
        public static IServiceCollection AddCacheClasses(this IServiceCollection services, Assembly assembly)
        {
            var allTypes = assembly.DefinedTypes
                .ToArray();

            var cacheType = typeof(CacheModule);
            foreach (var type in allTypes
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    cacheType.IsAssignableFrom(t.AsType())
                ))
            {
                services.AddTransient(type.AsType());
                services.AddTransient(cacheType, type.AsType());
            }

            return services;
        }

        public static void UseCacheManager(this IApplicationLifetime lifetime, IServiceScopeFactory factory, ILoggerFactory loggerFactory, RecurringBackgroundTask backgroundTask)
        {
            lifetime.UseBackgroundTask(factory, loggerFactory.CreateLogger<CacheManager>(), new RecurringBackgroundTask(
                "RefreshCache", 
                new TimeSpan(0, 1, 0), 
                (s, t) => s.GetService<ICacheManager>().CheckRefreshAsync(t)
            )
            {
                RunImmediately = true
            });
        }
    }
}