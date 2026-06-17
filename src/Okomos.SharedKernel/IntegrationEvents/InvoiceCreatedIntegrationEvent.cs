using Okomos.SharedKernel.Abstractions.Events;

namespace Okomos.SharedKernel.IntegrationEvents;

public sealed record InvoiceCreatedIntegrationEvent(
    Guid InvoiceId,
    Guid TenantId,
    decimal Amount) : IntegrationEvent;
