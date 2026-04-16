using ArkTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArkTracker.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<HoldingRecord> Holdings => Set<HoldingRecord>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<HoldingRecord>().HasIndex(h => h.Date);
            _ = modelBuilder.Entity<HoldingRecord>().HasIndex(h => h.IngestedAtUtc);
            _ = modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        }
    }
}
