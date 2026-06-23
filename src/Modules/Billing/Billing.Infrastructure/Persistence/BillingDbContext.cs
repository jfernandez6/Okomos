using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Okomos.Infrastructure.Persistence;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Billing.Infrastructure.Persistence;

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
