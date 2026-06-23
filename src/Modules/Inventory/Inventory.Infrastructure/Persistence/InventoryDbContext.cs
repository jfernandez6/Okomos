using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Okomos.Infrastructure.Persistence;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext : BaseDbContext
{
    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options,
        ITenantProvider tenantProvider) : base(options, tenantProvider)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
