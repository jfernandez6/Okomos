using Okomos.Accounting.Persistence.Entities;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Accounting.Persistence;

public sealed class AccountingDbContext : BaseDbContext
{
    public AccountingDbContext(
        DbContextOptions<AccountingDbContext> options,
        ITenantProvider tenantProvider,
        IEventBus eventBus) : base(options, tenantProvider, eventBus)
    {
    }

    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("accounting");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
