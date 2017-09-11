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
        /// <summary>
        /// Add the service registrations to enable cache management
        /// </summary>
        /// <param name="services">The Startup RegisterServices IServiceCollection</param>
        /// <returns>The cache builder for registering your expiration service</returns>
        public static ICacheManagerBuilder AddCacheManager(this IServiceCollection services)
        {
            var builder = new CacheManagerBuilder {Services = services};

            builder.Services.AddTransient<ICacheManager, CacheManager>();

            return builder;
        }

        /// <summary>
        /// Add your own custom ExpirationService to tell Cache Manager when to manually expire cache items
        /// </summary>
        /// <typeparam name="T">The ICacheExpirationService implementation to add to the DI container</typeparam>
        /// <param name="builder">The ICacheManagerBuilder provided by the AddCacheManager service registration</param>
        /// <returns>The expiration builder to add your custom cache registrations</returns>
        public static ICacheExpirationBuilder AddCustomExpirationService<T>(this ICacheManagerBuilder builder) where T : class, ICacheExpirationService
        {
            builder.Services.AddTransient<ICacheExpirationService, T>();
            return new CacheExpirationBuilder{ Builder = builder};
        }

        /// <summary>
        /// Add the Entity Framework ExpirationService to tell Cache Manager to expire cache when the database has a registered expiration
        /// </summary>
        /// <param name="builder">The ICacheManagerBuilder provided by the AddCacheManager service registration</param>
        /// <param name="contextBuilder">The ContextBuilder to enable </param>
        /// <returns>The expiration builder to add your custom cache registrations</returns>
        public static ICacheExpirationBuilder AddEfExpirationService(this ICacheManagerBuilder builder, Action<DbContextOptionsBuilder> contextBuilder)
        {
            builder.Services.AddSingleton(new EfCacheExpirationServiceOptions{EnableMigrations = true});
            builder.Services.AddTransient<ICacheExpirationService, EfCacheExpirationService>();
            builder.Services.AddDbContext<CacheExpirationContext>(contextBuilder);
            builder.Services.AddTransient<ICacheExpirationContext>(s => s.GetService<CacheExpirationContext>());

            return new CacheExpirationBuilder { Builder = builder };
        }

        /// <summary>
        /// Add the Entity Framework ExpirationService with your own custom ICacheExpirationContext to tell Cache Manager to expire cache when the database has a registered expiration
        /// </summary>
        /// <param name="builder">The ICacheManagerBuilder provided by the AddCacheManager service registration</param>
        /// <returns>The expiration builder to add your custom cache registrations</returns>
        public static ICacheExpirationBuilder AddCustomEfExpirationService<T>(this ICacheManagerBuilder builder) where T : class, ICacheExpirationContext
        {
            builder.Services.AddSingleton(new EfCacheExpirationServiceOptions { EnableMigrations = false });
            builder.Services.AddTransient<ICacheExpirationService, EfCacheExpirationService>();
            builder.Services.AddTransient<ICacheExpirationContext>(s => s.GetService<T>());

            return new CacheExpirationBuilder { Builder = builder };
        }

        /// <summary>
        /// Add a cache item by lambda method
        /// </summary>
        /// <typeparam name="T">The type of item being cached</typeparam>
        /// <param name="builder">The expiration builder provided by the Expiration Service Registartion</param>
        /// <param name="baseKey">The base key for this cache (keys must be unique to stop collisions)</param>
        /// <param name="lambda">The lambda factory method for your cached items</param>
        /// <returns>The expiration builder to add your custom cache registrations</returns>
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

        /// <summary>
        /// Auto register custom cache classes in an assembly
        /// </summary>
        /// <param name="builder">The expiration builder provided by the Expiration Service Registartion</param>
        /// <param name="assembly">The assembly to scan for CacheModules</param>
        /// <returns>The expiration builder to add your custom cache registrations</returns>
        public static ICacheExpirationBuilder AddCustomCache<TCacheType, T>(this ICacheExpirationBuilder builder, Assembly assembly) where TCacheType: CacheModule, ICache<T>
        {
            builder.Builder.Services.AddTransient<CacheModule, TCacheType>();
            builder.Builder.Services.AddTransient<ICache<T>, TCacheType>();

            return builder;
        }

        /// <summary>
        /// Auto register custom cache classes in an assembly
        /// </summary>
        /// <param name="builder">The expiration builder provided by the Expiration Service Registartion</param>
        /// <param name="assembly">The assembly to scan for CacheModules</param>
        /// <returns>The expiration builder to add your custom cache registrations</returns>
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

        /// <summary>
        /// This registers a job so your Expiration Service can periodically run to expire cache entries
        /// </summary>
        /// <param name="lifetime">The Application Lifetime that manages the start and stop of your application (this is uaually injected)</param>
        /// <param name="factory">The service scope so that cache items can be generated outside of request scopes</param>
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