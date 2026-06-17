using Okomos.Inventory.Persistence.Entities;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Inventory.Persistence;

public sealed class InventoryDbContext : BaseDbContext
{
    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options,
        ITenantProvider tenantProvider,
        IEventBus eventBus) : base(options, tenantProvider, eventBus)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
