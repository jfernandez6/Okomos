using FluentAssertions;
using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Behaviors.Transactions;
using Okomos.SharedKernel.Tests.TestHelpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Okomos.SharedKernel.Tests.Behaviors;

public class TransactionDecoratorTests : IAsyncLifetime
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
    public async Task Should_Commit_Transaction_When_Handler_Succeeds()
    {
        var inner = new PersistingCommandHandler(_dbContext);
        var decorator = new TransactionCommandDecorator<TestCommand, string, TestDbContext>(inner, _dbContext);

        var result = await decorator.HandleAsync(new TestCommand("persist"));

        result.Should().Be("persisted");
        (await _dbContext.TestEntities.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Should_Rollback_Transaction_When_Handler_Fails()
    {
        var inner = new FailingPersistingCommandHandler(_dbContext);
        var decorator = new TransactionCommandDecorator<TestCommand, string, TestDbContext>(inner, _dbContext);

        var act = () => decorator.HandleAsync(new TestCommand("fail"));

        await act.Should().ThrowAsync<InvalidOperationException>();
        (await _dbContext.TestEntities.CountAsync()).Should().Be(0);
    }

    private sealed class PersistingCommandHandler : ICommandHandler<TestCommand, string>
    {
        private readonly TestDbContext _dbContext;

        public PersistingCommandHandler(TestDbContext dbContext) => _dbContext = dbContext;

        public async Task<string> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            _dbContext.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = command.Value });
            await _dbContext.SaveChangesAsync(cancellationToken);
            return "persisted";
        }
    }

    private sealed class FailingPersistingCommandHandler : ICommandHandler<TestCommand, string>
    {
        private readonly TestDbContext _dbContext;

        public FailingPersistingCommandHandler(TestDbContext dbContext) => _dbContext = dbContext;

        public async Task<string> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            _dbContext.TestEntities.Add(new TestEntity { Id = Guid.NewGuid(), Name = command.Value });
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("fail");
        }
    }
}
