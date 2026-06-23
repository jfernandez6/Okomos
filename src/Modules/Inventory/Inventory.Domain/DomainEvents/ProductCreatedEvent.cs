using Okomos.SharedKernel.Abstractions.Events;

namespace Inventory.Domain.DomainEvents;

public sealed record ProductCreatedEvent(Guid ProductId, Guid TenantId, string Sku) : DomainEvent;
