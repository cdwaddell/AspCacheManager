using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Titanosoft.AspCacheManager.Data;

namespace Titanosoft.AspCacheManager
{
    //This is a utility class that allows migrations to be setup inside of a dotnet Standard library
    internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CacheExpirationContext>
    {
        public CacheExpirationContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CacheExpirationContext>();

            builder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=CacheExpiration;Trusted_Connection=True;MultipleActiveResultSets=true");

            return new CacheExpirationContext(builder.Options);
        }
    }
}
