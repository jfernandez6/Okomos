using Accounting.Application.Features.GetJournalEntryById;
using Accounting.Domain.Entities;
using Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Tests.Features;

public class GetJournalEntryByIdHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Entry_When_Exists()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AccountingDbContext(options, tenantProvider);
        var entry = JournalEntry.Create(tenantId, "Query test", 200m, 200m);
        dbContext.JournalEntries.Add(entry);
        await dbContext.SaveChangesAsync();

        var handler = new GetJournalEntryByIdHandler(dbContext);
        var result = await handler.Handle(
            new GetJournalEntryByIdRequest { EntryId = entry.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Debit.Should().Be(200m);
        result.Credit.Should().Be(200m);
    }
}
