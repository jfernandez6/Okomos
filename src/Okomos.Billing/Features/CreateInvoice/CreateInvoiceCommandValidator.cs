using FastEndpoints;

namespace Okomos.Billing.Features.CreateInvoice;

public sealed class CreateInvoiceCommandValidator : Validator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Customer name is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
