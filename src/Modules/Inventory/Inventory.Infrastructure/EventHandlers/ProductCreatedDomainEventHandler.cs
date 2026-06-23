using Inventory.Domain.DomainEvents;
using Inventory.Infrastructure.Persistence;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Outbox;
using Okomos.SharedKernel.IntegrationEvents;

namespace Inventory.Infrastructure.EventHandlers;

public sealed class ProductCreatedDomainEventHandler : IDomainEventHandler<ProductCreatedEvent>
{
    private readonly IOutboxStore<InventoryDbContext> _outboxStore;

    public ProductCreatedDomainEventHandler(IOutboxStore<InventoryDbContext> outboxStore)
    {
        _outboxStore = outboxStore;
    }

    public async Task HandleAsync(ProductCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var integrationEvent = new ProductCreatedIntegrationEvent(
            domainEvent.ProductId,
            domainEvent.TenantId,
            domainEvent.Sku);

        await _outboxStore.AddAsync(integrationEvent, domainEvent.TenantId, cancellationToken);
    }
}
