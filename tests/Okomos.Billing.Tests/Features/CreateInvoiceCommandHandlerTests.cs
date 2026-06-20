using FluentAssertions;
using Okomos.Billing.Features.CreateInvoice;
using Okomos.Billing.Persistence;
using Okomos.Billing.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Billing.Tests.Features;

public class CreateInvoiceCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Create_Invoice_And_Raise_Domain_Event()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new BillingDbContext(options, tenantProvider);
        var handler = new CreateInvoiceCommandHandler(dbContext, tenantProvider);

        var command = new CreateInvoiceCommand("Acme Corp", 500m, "MXN");
        var invoiceId = await handler.HandleAsync(command);

        invoiceId.Should().NotBeEmpty();

        var invoice = await dbContext.Invoices.SingleAsync();
        invoice.CustomerName.Should().Be("Acme Corp");
        invoice.Amount.Should().Be(500m);
        invoice.TenantId.Should().Be(tenantId);
        invoice.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<InvoiceCreatedEvent>();
    }
}
