using Billing.Application.Features.CreateInvoice;
using Billing.Application.Features.GetInvoiceById;
using Billing.Infrastructure.Persistence;
using Billing.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okomos.Infrastructure.Events;
using Okomos.Infrastructure.Persistence;

namespace Billing.Tests.Integration;

[Collection(nameof(BillingSqlServerCollection))]
public class BillingIntegrationTests
{
    private readonly BillingSqlServerFixture _fixture;

    public BillingIntegrationTests(BillingSqlServerFixture fixture) => _fixture = fixture;

    [DockerFact]
    public async Task Should_Create_And_Get_Invoice_Using_SqlServer()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using var dbContext = new BillingDbContext(options, tenantProvider);

        var unitOfWork = new UnitOfWork<BillingDbContext>(
            dbContext,
            new TestEventBus(),
            Substitute.For<ILogger<DomainEventDispatcher>>());

        var createHandler = new CreateInvoiceHandler(dbContext, tenantProvider, unitOfWork);
        var createResult = await createHandler.Handle(
            new CreateInvoiceRequest("SQL Customer", 1200m, "MXN"), CancellationToken.None);

        var queryHandler = new GetInvoiceByIdHandler(dbContext);
        var invoice = await queryHandler.Handle(
            new GetInvoiceByIdRequest { InvoiceId = createResult.Id }, CancellationToken.None);

        invoice.Should().NotBeNull();
        invoice!.CustomerName.Should().Be("SQL Customer");
        invoice.Amount.Should().Be(1200m);
        invoice.TenantId.Should().Be(tenantId);
    }
}
