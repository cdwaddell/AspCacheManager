using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Titanosoft.AspBackgroundWorker;
using Titanosoft.AspCacheManager.Data;

namespace Titanosoft.AspCacheManager
{
    public static class ServiceExtensions
    {
        public static ICacheManagerBuilder AddCacheManager(this IServiceCollection services)
        {
            var builder = new CacheManagerBuilder {Services = services};

            builder.Services.AddTransient<ICacheManager, CacheManager>();

            return builder;
        }

        public static ICacheExpirationBuilder AddCustomExpirationService<T>(this ICacheManagerBuilder builder) where T : class, ICacheExpirationService
        {
            builder.Services.AddTransient<ICacheExpirationService, T>();
            return new CacheExpirationBuilder{ Builder = builder};
        }

        public static ICacheExpirationBuilder AddEfExpirationService(this ICacheManagerBuilder builder, Action<DbContextOptionsBuilder> contextBuilder)
        {
            builder.AddCustomExpirationService<EfCacheExpirationService>();
            builder.Services.AddDbContext<CacheExpirationContext>(contextBuilder);
            builder.Services.AddTransient<ICacheExpirationContext>(s => s.GetService<CacheExpirationContext>());

            return new CacheExpirationBuilder { Builder = builder };
        }

        public static ICacheExpirationBuilder AddLambdaCache<T>(this ICacheExpirationBuilder builder, string baseKey, Func<IDictionary<string, string>, CancellationToken, Task<T>> lambda) where T : class
        {
            if (LambdaCacheModule<T>.BaseKey != null) throw new ArgumentException("Cannot register the same type twice using lambda statements");
            if (LambdaCacheModule<T>.LambdaFactory != null) throw new ArgumentException("Cannot register the same type twice using lambda statements");

            LambdaCacheModule<T>.LambdaFactory = lambda ?? throw new ArgumentNullException(nameof(lambda));
            LambdaCacheModule<T>.BaseKey = baseKey ?? throw new ArgumentNullException(nameof(baseKey));

            builder.Builder.Services.AddTransient<CacheModule, LambdaCacheModule<T>>();
            builder.Builder.Services.AddTransient<ICache<T>, LambdaCacheModule<T>>();

            return builder;
        }

        public static ICacheExpirationBuilder AddCacheClasses(this ICacheExpirationBuilder builder, Assembly assembly)
        {
            var allTypes = assembly.DefinedTypes
                .ToArray();

            var cacheType = typeof(CacheModule);
            foreach (var type in allTypes
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    cacheType.IsAssignableFrom(t.AsType())
                ).Select(t => t.AsType()))
            {
                var iCacheType = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICache<>))
                    .SelectMany(i => i.GetGenericArguments())
                    .First();
                builder.Builder.Services.AddTransient(iCacheType, type);
                builder.Builder.Services.AddTransient(type);
                builder.Builder.Services.AddTransient(cacheType, type);
            }

            return builder;
        }

        public static void UseCacheManager(this IApplicationLifetime lifetime, IServiceScopeFactory factory)
        {
            using (var scope = factory.CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<CacheManager>>();
                lifetime.UseBackgroundTask(factory, logger,
                    new RecurringBackgroundTask(
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
}