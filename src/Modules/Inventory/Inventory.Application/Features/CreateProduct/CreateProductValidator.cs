using FastEndpoints;
using FluentValidation;

namespace Inventory.Application.Features.CreateProduct;

public sealed class CreateProductValidator : Validator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Sku).NotEmpty().WithMessage("SKU is required.");
        RuleFor(x => x.QuantityOnHand).GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
    }
}
