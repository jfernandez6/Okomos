using Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Okomos.Infrastructure.Persistence;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Accounting.Infrastructure.Persistence;

public sealed class AccountingDbContext : BaseDbContext
{
    public AccountingDbContext(
        DbContextOptions<AccountingDbContext> options,
        ITenantProvider tenantProvider) : base(options, tenantProvider)
    {
    }

    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
