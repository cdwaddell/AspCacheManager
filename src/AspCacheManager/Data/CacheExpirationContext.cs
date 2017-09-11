using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Titanosoft.AspCacheManager.Data
{
    public interface ICacheExpirationContext
    {
        DbSet<ExpiredCacheKey> ExpiredCacheKeys { get; set; }
        int SaveChanges();
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }

    public class CacheExpirationContext: DbContext, ICacheExpirationContext
    {
        public CacheExpirationContext(DbContextOptions<CacheExpirationContext> options) : base(options)
        {
            
        }

        public DbSet<ExpiredCacheKey> ExpiredCacheKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ExpiredCacheKey>(t =>
            {
                t.Property(x => x.JsonCacheKey)
                    .IsRequired();

                t.Property(x => x.UtcExpiration)
                    .HasDefaultValueSql("GETUTCDATE()");

                t.ToTable("ExpiredCacheKeys", "cache");
            });
        }
    }
}
