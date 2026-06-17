using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Billing.Features.CreateInvoice;

public sealed record CreateInvoiceCommand(
    string CustomerName,
    decimal Amount,
    string Currency = "USD") : ICommand<Guid>;
