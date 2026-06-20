using FluentAssertions;
using Okomos.SharedKernel.Abstractions.Outbox;
using Okomos.SharedKernel.IntegrationEvents;
using Okomos.SharedKernel.Persistence;
using Okomos.SharedKernel.Tests.TestHelpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Okomos.SharedKernel.Tests.Outbox;

public class OutboxStoreTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private TestDbContext _dbContext = null!;
    private OutboxStore<TestDbContext> _outboxStore = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new TestDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _outboxStore = new OutboxStore<TestDbContext>(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_Should_Add_Outbox_Message_To_Change_Tracker()
    {
        var tenantId = Guid.NewGuid();
        var integrationEvent = new InvoiceCreatedIntegrationEvent(Guid.NewGuid(), tenantId, 100m);

        await _outboxStore.AddAsync(integrationEvent, tenantId);
        await _dbContext.SaveChangesAsync();

        var messages = await _dbContext.OutboxMessages.ToListAsync();
        messages.Should().ContainSingle();
        messages[0].EventType.Should().Be(nameof(InvoiceCreatedIntegrationEvent));
        messages[0].TenantId.Should().Be(tenantId);
        messages[0].ProcessedOn.Should().BeNull();
    }

    [Fact]
    public async Task GetPendingAsync_Should_Return_Unprocessed_Messages()
    {
        var integrationEvent = new InvoiceCreatedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), 50m);
        await _outboxStore.AddAsync(integrationEvent, Guid.NewGuid());
        await _dbContext.SaveChangesAsync();

        var pending = await _outboxStore.GetPendingAsync(10);

        pending.Should().HaveCount(1);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_Should_Set_ProcessedOn()
    {
        var integrationEvent = new InvoiceCreatedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), 75m);
        await _outboxStore.AddAsync(integrationEvent, Guid.NewGuid());
        await _dbContext.SaveChangesAsync();
        var messageId = (await _dbContext.OutboxMessages.SingleAsync()).Id;

        await _outboxStore.MarkAsProcessedAsync(messageId);

        var message = await _dbContext.OutboxMessages.SingleAsync();
        message.ProcessedOn.Should().NotBeNull();
        (await _outboxStore.GetPendingAsync(10)).Should().BeEmpty();
    }

    [Fact]
    public async Task MarkAsFailedAsync_Should_Set_Error()
    {
        var integrationEvent = new InvoiceCreatedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), 25m);
        await _outboxStore.AddAsync(integrationEvent, Guid.NewGuid());
        await _dbContext.SaveChangesAsync();
        var messageId = (await _dbContext.OutboxMessages.SingleAsync()).Id;

        await _outboxStore.MarkAsFailedAsync(messageId, "test error");

        var message = await _dbContext.OutboxMessages.SingleAsync();
        message.Error.Should().Be("test error");
    }
}
