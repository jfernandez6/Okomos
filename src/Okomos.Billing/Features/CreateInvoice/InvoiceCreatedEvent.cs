using Okomos.SharedKernel.Abstractions.Events;

namespace Okomos.Billing.Features.CreateInvoice;

public sealed record InvoiceCreatedEvent(Guid InvoiceId, Guid TenantId, decimal Amount) : DomainEvent;
