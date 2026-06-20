using FluentAssertions;
using Okomos.Accounting.Features.CreateJournalEntry;
using Okomos.Accounting.Persistence;
using Okomos.Accounting.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Accounting.Tests.Features;

public class CreateJournalEntryCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Create_JournalEntry()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AccountingDbContext(options, tenantProvider);
        var handler = new CreateJournalEntryCommandHandler(dbContext, tenantProvider);

        var entryId = await handler.HandleAsync(new CreateJournalEntryCommand("Test entry", 100m, 100m));

        entryId.Should().NotBeEmpty();
        var entry = await dbContext.JournalEntries.SingleAsync();
        entry.Description.Should().Be("Test entry");
        entry.TenantId.Should().Be(tenantId);
        entry.DomainEvents.Should().ContainSingle();
    }
}
