using FluentAssertions;
using Okomos.SharedKernel.Abstractions.Entities;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Okomos.SharedKernel.Tests.Events;

public class BaseDbContextDomainEventsTests
{
    [Fact]
    public async Task SaveChanges_Should_Dispatch_Domain_Events()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.CurrentTenantId.Returns(tenantId);

        var eventBus = Substitute.For<IEventBus>();

        var options = new DbContextOptionsBuilder<DomainTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new DomainTestDbContext(options, tenantProvider, eventBus);
        var entity = DomainTestEntity.Create("test");
        context.DomainEntities.Add(entity);

        await context.SaveChangesAsync();

        await eventBus.Received(1).PublishDomainEventAsync(
            Arg.Is<IDomainEvent>(e => e is EntityCreatedEvent),
            Arg.Any<CancellationToken>());
        entity.DomainEvents.Should().BeEmpty();
    }

    private sealed class DomainTestDbContext : BaseDbContext
    {
        public DomainTestDbContext(
            DbContextOptions<DomainTestDbContext> options,
            ITenantProvider tenantProvider,
            IEventBus eventBus) : base(options, tenantProvider, eventBus) { }

        public DbSet<DomainTestEntity> DomainEntities => Set<DomainTestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DomainTestEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }
    }

    private sealed class DomainTestEntity : Entity, ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; } = string.Empty;

        public static DomainTestEntity Create(string name)
        {
            var entity = new DomainTestEntity
            {
                Id = Guid.NewGuid(),
                Name = name
            };
            entity.AddDomainEvent(new EntityCreatedEvent(entity.Id));
            return entity;
        }
    }

    private sealed record EntityCreatedEvent(Guid EntityId) : DomainEvent;
}
