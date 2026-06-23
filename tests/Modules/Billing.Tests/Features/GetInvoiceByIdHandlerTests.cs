using Billing.Application.Features.GetInvoiceById;
using Billing.Domain.Entities;
using Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Billing.Tests.Features;

public class GetInvoiceByIdHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Invoice_When_Exists()
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

        var handler = new GetInvoiceByIdHandler(dbContext);
        var result = await handler.Handle(new GetInvoiceByIdRequest { InvoiceId = invoice.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(invoice.Id);
        result.CustomerName.Should().Be("Customer");
        result.Amount.Should().Be(250m);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Not_Found()
    {
        var tenantProvider = new TestTenantProvider(Guid.NewGuid());

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new BillingDbContext(options, tenantProvider);
        var handler = new GetInvoiceByIdHandler(dbContext);

        var result = await handler.Handle(new GetInvoiceByIdRequest { InvoiceId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeNull();
    }
}
