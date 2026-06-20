using FluentAssertions;
using Okomos.Billing.Features.GetInvoiceById;
using Okomos.Billing.Persistence;
using Okomos.Billing.Persistence.Entities;
using Okomos.Billing.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Billing.Tests.Features;

public class GetInvoiceByIdQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Invoice_When_Exists()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new BillingDbContext(options, tenantProvider);

        var invoice = Invoice.Create(tenantId, "Customer", 250m, "USD");
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();

        var handler = new GetInvoiceByIdQueryHandler(dbContext);
        var result = await handler.HandleAsync(new GetInvoiceByIdQuery(invoice.Id));

        result.Should().NotBeNull();
        result!.Id.Should().Be(invoice.Id);
        result.CustomerName.Should().Be("Customer");
        result.Amount.Should().Be(250m);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Null_When_Not_Found()
    {
        var tenantProvider = new TestTenantProvider(Guid.NewGuid());

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new BillingDbContext(options, tenantProvider);
        var handler = new GetInvoiceByIdQueryHandler(dbContext);

        var result = await handler.HandleAsync(new GetInvoiceByIdQuery(Guid.NewGuid()));

        result.Should().BeNull();
    }
}
