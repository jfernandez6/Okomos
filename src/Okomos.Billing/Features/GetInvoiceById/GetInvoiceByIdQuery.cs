using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Billing.Features.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IQuery<InvoiceDto?>;

public sealed record InvoiceDto(
    Guid Id,
    Guid TenantId,
    string CustomerName,
    decimal Amount,
    string Currency,
    DateTime IssuedAt,
    string Status);
