using FluentAssertions;
using Okomos.Accounting.Features.CreateJournalEntry;
using Okomos.Accounting.Features.GetJournalEntryById;
using Okomos.Accounting.Persistence;
using Okomos.Accounting.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Accounting.Tests.Integration;

[Collection(nameof(AccountingSqlServerCollection))]
public class AccountingIntegrationTests
{
    private readonly AccountingSqlServerFixture _fixture;

    public AccountingIntegrationTests(AccountingSqlServerFixture fixture) => _fixture = fixture;

    [DockerFact]
    public async Task Should_Create_And_Get_JournalEntry_Using_SqlServer()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using var dbContext = new AccountingDbContext(options, tenantProvider);

        var createHandler = new CreateJournalEntryCommandHandler(dbContext, tenantProvider);
        var entryId = await createHandler.HandleAsync(new CreateJournalEntryCommand("SQL Entry", 500m, 500m));

        var queryHandler = new GetJournalEntryByIdQueryHandler(dbContext);
        var entry = await queryHandler.HandleAsync(new GetJournalEntryByIdQuery(entryId));

        entry.Should().NotBeNull();
        entry!.Description.Should().Be("SQL Entry");
    }
}
