namespace Billing.Application.Features.GetInvoiceById;

public sealed record InvoiceDto(
    Guid Id,
    Guid TenantId,
    string CustomerName,
    decimal Amount,
    string Currency,
    DateTime IssuedAt,
    string Status);
