using Okomos.SharedKernel.Abstractions.Entities;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Abstractions.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Okomos.SharedKernel.Persistence;

public abstract class BaseDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    protected BaseDbContext(
        DbContextOptions options,
        ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Payload).IsRequired();
            entity.HasIndex(e => new { e.ProcessedOn, e.OccurredOn });
        });

        ApplyTenantQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(BaseDbContext)
                    .GetMethod(nameof(SetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(this, [modelBuilder]);
            }
        }
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            _tenantProvider.CurrentTenantId == null || e.TenantId == _tenantProvider.CurrentTenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantIdToNewEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantIdToNewEntities()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (tenantId is null) return;

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>()
                     .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty))
        {
            entry.Entity.TenantId = tenantId.Value;
        }
    }
}
