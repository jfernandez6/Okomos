using Okomos.SharedKernel.Behaviors.Validation;

namespace Okomos.Billing.Features.CreateInvoice;

public sealed class CreateInvoiceCommandValidator : IValidator<CreateInvoiceCommand>
{
    public Task<ValidationResult> ValidateAsync(CreateInvoiceCommand request, CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(request.CustomerName))
            result.AddError(nameof(request.CustomerName), "Customer name is required.");

        if (request.Amount <= 0)
            result.AddError(nameof(request.Amount), "Amount must be greater than zero.");

        return Task.FromResult(result);
    }
}
