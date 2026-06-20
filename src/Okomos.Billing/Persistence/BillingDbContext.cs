using Okomos.Billing.Persistence.Entities;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Billing.Persistence;

public sealed class BillingDbContext : BaseDbContext
{
    public BillingDbContext(
        DbContextOptions<BillingDbContext> options,
        ITenantProvider tenantProvider) : base(options, tenantProvider)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
