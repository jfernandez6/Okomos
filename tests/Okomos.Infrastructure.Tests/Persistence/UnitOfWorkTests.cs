using Okomos.Infrastructure.Events;
using Okomos.Infrastructure.Persistence;
using Okomos.Infrastructure.Tests.TestHelpers;
using Okomos.SharedKernel.Abstractions.Events;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Okomos.Infrastructure.Tests.Persistence;

public class UnitOfWorkTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private TestDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new TestDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Commit_When_Save_Succeeds()
    {
        var eventBus = new TestEventBus();
        var unitOfWork = new UnitOfWork<TestDbContext>(
            _dbContext,
            eventBus,
            Substitute.For<ILogger<DomainEventDispatcher>>());

        _dbContext.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = "persist" });
        await unitOfWork.SaveChangesAsync();

        (await _dbContext.TestEntities.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Rollback_When_Domain_Event_Dispatch_Fails()
    {
        var eventBus = Substitute.For<IEventBus>();
        eventBus.PublishDomainEventAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("fail"));

        var unitOfWork = new UnitOfWork<TestDbContext>(
            _dbContext,
            eventBus,
            Substitute.For<ILogger<DomainEventDispatcher>>());

        var aggregate = new TestAggregate { Id = Guid.NewGuid(), Name = "fail" };
        aggregate.AddDomainEvent(new TestAggregateEvent());
        _dbContext.TestAggregates.Add(aggregate);

        var act = () => unitOfWork.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
        (await _dbContext.TestAggregates.CountAsync()).Should().Be(0);
    }
}
