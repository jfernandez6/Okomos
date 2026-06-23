using FastEndpoints;

namespace Okomos.Inventory.Features.CreateProduct;

public sealed class CreateProductCommandValidator : Validator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Sku).NotEmpty().WithMessage("SKU is required.");
        RuleFor(x => x.QuantityOnHand).GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
    }
}
