using Okomos.SharedKernel.Behaviors.Validation;

namespace Okomos.Inventory.Features.CreateProduct;

public sealed class CreateProductCommandValidator : IValidator<CreateProductCommand>
{
    public Task<ValidationResult> ValidateAsync(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(request.Name))
            result.AddError(nameof(request.Name), "Name is required.");

        if (string.IsNullOrWhiteSpace(request.Sku))
            result.AddError(nameof(request.Sku), "SKU is required.");

        if (request.QuantityOnHand < 0)
            result.AddError(nameof(request.QuantityOnHand), "Quantity cannot be negative.");

        return Task.FromResult(result);
    }
}
