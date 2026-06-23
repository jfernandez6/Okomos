using Inventory.Application.Features.CreateProduct;

namespace Inventory.Tests.Features;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_Sku_Is_Empty()
    {
        var result = await _validator.ValidateAsync(new CreateProductRequest("Name", "", 1, 10m));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Sku));
    }

    [Fact]
    public async Task Should_Pass_When_Valid()
    {
        var result = await _validator.ValidateAsync(new CreateProductRequest("Name", "SKU-1", 1, 10m));

        result.IsValid.Should().BeTrue();
    }
}
