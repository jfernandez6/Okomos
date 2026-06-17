using Okomos.SharedKernel.Abstractions.Events;

namespace Okomos.Inventory.Features.CreateProduct;

public sealed record ProductCreatedEvent(Guid ProductId, Guid TenantId, string Sku) : DomainEvent;
