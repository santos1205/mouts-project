using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Infrastructure.Persistence;

public class DeveloperStoreDbContext : DbContext
{
    public DeveloperStoreDbContext(DbContextOptions<DeveloperStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeveloperStoreDbContext).Assembly);
    }
}
