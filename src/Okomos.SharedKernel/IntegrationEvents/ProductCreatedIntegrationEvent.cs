using Okomos.SharedKernel.Abstractions.Events;

namespace Okomos.SharedKernel.IntegrationEvents;

public sealed record ProductCreatedIntegrationEvent(
    Guid ProductId,
    Guid TenantId,
    string Sku) : IntegrationEvent;
