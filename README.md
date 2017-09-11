# Titanosoft.AspCacheManager

![Build Status](https://cdanielwaddell.visualstudio.com/_apis/public/build/definitions/991b95e6-1640-4127-b933-3b0aaddb919b/8/badge)

### What is AspCacheManager?

AspCacheManager is a dotnet Standard 2.0 library for creating a cache management strategy. This package allows you to prehydrate cache, and proactively rehydrate cached items, so your users never have to wait for an expired cache item to rehydrate with updated data.

### How do I get started?

First install the nuget package:

```
PM> Install-Package Titanosoft.AspCacheManager
```

Once installed, you need to register the module in your ConfigureServices method:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        var migrationsAssembly = typeof(ServiceExtensions).GetTypeInfo().Assembly.GetName().Name;

        //services.AddDbContext<MyCustomDbContext>();
        //services.AddTransient<MyExpirationService>();
        services.AddTransient<SomeOtherProject.MyCacheClass>();

        services
            //Add cache management
            .AddCacheManager()

            //register a class that determines when cache should be forced to expire
            //Option 1 - Accept add defaults (even connection string)
            .AddEfExpirationService()

            ////Option 2 - Change the connection string
            //.AddEfExpirationService("MyConnectionString")

            ////Option 3 - If you want more control
            //.AddEfExpirationService(x =>
            //    x.UseSqlServer(connectionString, options =>
            //        options.MigrationsAssembly(migrationsAssembly)
            //    )
            //)

            ////Option 4 - If you created your own context that implements from ICacheExpirationContext
            ////NOTE: MyCustomDbContext needs to already be registered with the DI container
            ////NOTE 2: You will have to manually handle migrations of your own context
            //.AddCustomEfExpirationService<MyCustomDbContext>

            ////Option 5 - If you want custom logic and/or no database envolved
            ////NOTE: MyExpirationService needs to already be registered with the DI container
            //AddCustomExpirationService<MyExpirationService>)

            //Manually register a custom cache item
            ////NOTE: SomeOtherProject.MyCacheClass needs to already be registered with the DI container
            .AddCustomCache<SomeOtherProject.MyCacheClass>()
            //--And/Or--
            //Autoscan an assembly for cache instances
            .AddCacheClasses(typeof(Startup).GetTypeInfo().Assembly)
    }
    ...
}
```

Then, you need to configure the module to scan for cache expirations in the Confgure method:

```csharp
public void Configure(IApplicationBuilder app)
{
    ...
    //Register a background job to monitor cache items for you
    app.UseCacheManager()
        //If you used AddEfExpirationService to register and enabled migrations (on by default)
        //you can have the application auto apply migrations
        .CheckMigrations();
    ...
}
```

### What if I want to use your context, but don't want to give full db access to the user?

Don't worry, you don't have to. You can just give full schema access. Use the following sql script to create the user for the connection given to the configuration connection string. This will isolate your other schemas from being affected by the library:

```sql
CREATE USER [AspCacheManager] WITH PASSWORD = 'strong_password_for_config'
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = 'cache') EXEC ('CREATE SCHEMA [cache]')
GO

ALTER AUTHORIZATION ON SCHEMA::[cache] TO [AspCacheManager]
GO

GRANT CREATE TABLE TO [AspCacheManager]
GO
```

### What if I want to use your context, but I don't want to automigrate at all?

You also don't have to do this, but it is a little more work. Inside of the nuget package is a "tools" folder. In that folder is a "Migrations.sql" file. If you retreive that file and run it as part of your deployment, your database will always be up to date. Note: the user running the deployment script will have to have the above access.
