using FastEndpoints;
using FluentValidation;

namespace Billing.Application.Features.CreateInvoice;

public sealed class CreateInvoiceValidator : Validator<CreateInvoiceRequest>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Customer name is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
