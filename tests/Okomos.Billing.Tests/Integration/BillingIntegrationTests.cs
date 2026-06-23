using FluentAssertions;
using Okomos.Billing.Features.CreateInvoice;
using Okomos.Billing.Features.GetInvoiceById;
using Okomos.Billing.Persistence;
using Okomos.Billing.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Billing.Tests.Integration;

[Collection(nameof(BillingSqlServerCollection))]
public class BillingIntegrationTests
{
    private readonly BillingSqlServerFixture _fixture;

    public BillingIntegrationTests(BillingSqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [DockerFact]
    public async Task Should_Create_And_Get_Invoice_Using_SqlServer()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using var dbContext = new BillingDbContext(options, tenantProvider);

        var createHandler = new CreateInvoiceCommandHandler(dbContext, tenantProvider);
        var invoiceId = await createHandler.HandleAsync(new CreateInvoiceCommand("SQL Customer", 1200m, "MXN"));
        await dbContext.SaveChangesAsync();

        var queryHandler = new GetInvoiceByIdQueryHandler(dbContext);
        var invoice = await queryHandler.HandleAsync(new GetInvoiceByIdQuery(invoiceId));

        invoice.Should().NotBeNull();
        invoice!.CustomerName.Should().Be("SQL Customer");
        invoice.Amount.Should().Be(1200m);
        invoice.TenantId.Should().Be(tenantId);
    }
}
