using Okomos.Identity.Persistence.Entities;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Abstractions.Outbox;
using Okomos.SharedKernel.Persistence;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Identity.Persistence;

public sealed class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IEventBus _eventBus;

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        ITenantProvider tenantProvider,
        IEventBus eventBus) : base(options)
    {
        _tenantProvider = tenantProvider;
        _eventBus = eventBus;
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();
        });

        builder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages", "identity");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Payload).IsRequired();
            entity.HasIndex(e => new { e.ProcessedOn, e.OccurredOn });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantIdToNewUsers();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantIdToNewUsers()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (tenantId is null) return;

        foreach (var entry in ChangeTracker.Entries<ApplicationUser>()
                     .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty))
        {
            entry.Entity.TenantId = tenantId.Value;
        }
    }
}
