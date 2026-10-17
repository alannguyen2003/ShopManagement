using Microsoft.EntityFrameworkCore;

namespace ShopManagement.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AdminAccountRecord> AdminAccounts => Set<AdminAccountRecord>();

    public DbSet<AdminAccountRow> AdminAccountRows => Set<AdminAccountRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
