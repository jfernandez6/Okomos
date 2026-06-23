using Accounting.Application.Features.CreateJournalEntry;
using Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okomos.Infrastructure.Events;
using Okomos.Infrastructure.Persistence;

namespace Accounting.Tests.Features;

public class CreateJournalEntryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_JournalEntry()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseTransactionalInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AccountingDbContext(options, tenantProvider);
        var unitOfWork = new UnitOfWork<AccountingDbContext>(
            dbContext,
            new TestEventBus(),
            Substitute.For<ILogger<DomainEventDispatcher>>());

        var handler = new CreateJournalEntryHandler(dbContext, tenantProvider, unitOfWork);

        var result = await handler.Handle(
            new CreateJournalEntryRequest("Test entry", 100m, 100m), CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        var entry = dbContext.JournalEntries.Local.Single();
        entry.Description.Should().Be("Test entry");
        entry.TenantId.Should().Be(tenantId);
    }
}
