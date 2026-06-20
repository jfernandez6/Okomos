using Okomos.Billing.Features.CreateInvoice;
using Okomos.Billing.Persistence;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Outbox;
using Okomos.SharedKernel.IntegrationEvents;

namespace Okomos.Billing.EventHandlers;

public sealed class InvoiceCreatedDomainEventHandler : IDomainEventHandler<InvoiceCreatedEvent>
{
    private readonly IOutboxStore<BillingDbContext> _outboxStore;

    public InvoiceCreatedDomainEventHandler(IOutboxStore<BillingDbContext> outboxStore)
    {
        _outboxStore = outboxStore;
    }

    public async Task HandleAsync(InvoiceCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var integrationEvent = new InvoiceCreatedIntegrationEvent(
            domainEvent.InvoiceId,
            domainEvent.TenantId,
            domainEvent.Amount);

        await _outboxStore.AddAsync(integrationEvent, domainEvent.TenantId, cancellationToken);
    }
}
