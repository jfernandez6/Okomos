using Accounting.Application.Features.CreateJournalEntry;
using Accounting.Application.Features.GetJournalEntryById;
using Accounting.Infrastructure.Persistence;
using Accounting.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okomos.Infrastructure.Events;
using Okomos.Infrastructure.Persistence;

namespace Accounting.Tests.Integration;

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

        var unitOfWork = new UnitOfWork<AccountingDbContext>(
            dbContext,
            new TestEventBus(),
            Substitute.For<ILogger<DomainEventDispatcher>>());

        var createHandler = new CreateJournalEntryHandler(dbContext, tenantProvider, unitOfWork);
        var createResult = await createHandler.Handle(
            new CreateJournalEntryRequest("SQL Entry", 500m, 500m), CancellationToken.None);

        var queryHandler = new GetJournalEntryByIdHandler(dbContext);
        var entry = await queryHandler.Handle(
            new GetJournalEntryByIdRequest { EntryId = createResult.Id }, CancellationToken.None);

        entry.Should().NotBeNull();
        entry!.Description.Should().Be("SQL Entry");
    }
}
