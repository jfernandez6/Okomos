using Okomos.SharedKernel.Abstractions.Entities;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Infrastructure.Tests.TestHelpers;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    public DbSet<TestAggregate> TestAggregates => Set<TestAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.Payload).IsRequired();
        });

        modelBuilder.Entity<TestEntity>(entity => entity.HasKey(e => e.Id));
        modelBuilder.Entity<TestAggregate>(entity => entity.HasKey(e => e.Id));
    }
}

public sealed class TestEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class TestAggregate : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed record TestAggregateEvent : DomainEvent;
