using Billing.Application.Features.CreateInvoice;
using Billing.Domain.DomainEvents;
using Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okomos.Infrastructure.Events;
using Okomos.Infrastructure.Persistence;

namespace Billing.Tests.Features;

public class CreateInvoiceHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Invoice_And_Dispatch_Domain_Event()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);
        var eventBus = new TestEventBus();

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseTransactionalInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new BillingDbContext(options, tenantProvider);
        var unitOfWork = new UnitOfWork<BillingDbContext>(
            dbContext,
            eventBus,
            Substitute.For<ILogger<DomainEventDispatcher>>());

        var handler = new CreateInvoiceHandler(dbContext, tenantProvider, unitOfWork);

        var result = await handler.Handle(new CreateInvoiceRequest("Acme Corp", 500m, "MXN"), CancellationToken.None);

        result.Id.Should().NotBeEmpty();

        var invoice = dbContext.Invoices.Local.Single();
        invoice.CustomerName.Should().Be("Acme Corp");
        invoice.Amount.Should().Be(500m);
        invoice.TenantId.Should().Be(tenantId);
        eventBus.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<InvoiceCreatedEvent>();
    }
}
