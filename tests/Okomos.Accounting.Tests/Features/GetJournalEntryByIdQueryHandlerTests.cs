using FluentAssertions;
using Okomos.Accounting.Features.GetJournalEntryById;
using Okomos.Accounting.Persistence;
using Okomos.Accounting.Persistence.Entities;
using Okomos.Accounting.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Accounting.Tests.Features;

public class GetJournalEntryByIdQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Entry_When_Exists()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);
        var eventBus = new TestEventBus();

        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AccountingDbContext(options, tenantProvider, eventBus);
        var entry = JournalEntry.Create(tenantId, "Query test", 200m, 200m);
        dbContext.JournalEntries.Add(entry);
        await dbContext.SaveChangesAsync();

        var handler = new GetJournalEntryByIdQueryHandler(dbContext);
        var result = await handler.HandleAsync(new GetJournalEntryByIdQuery(entry.Id));

        result.Should().NotBeNull();
        result!.Debit.Should().Be(200m);
        result.Credit.Should().Be(200m);
    }
}
