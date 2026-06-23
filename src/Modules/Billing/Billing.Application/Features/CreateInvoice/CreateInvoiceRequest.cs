namespace Billing.Application.Features.CreateInvoice;

public sealed record CreateInvoiceRequest(string CustomerName, decimal Amount, string Currency = "USD");
