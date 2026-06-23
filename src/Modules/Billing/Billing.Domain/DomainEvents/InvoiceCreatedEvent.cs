using Okomos.SharedKernel.Abstractions.Events;

namespace Billing.Domain.DomainEvents;

public sealed record InvoiceCreatedEvent(Guid InvoiceId, Guid TenantId, decimal Amount) : DomainEvent;
