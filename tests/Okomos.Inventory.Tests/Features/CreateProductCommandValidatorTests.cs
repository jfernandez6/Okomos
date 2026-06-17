using FluentAssertions;
using Okomos.Inventory.Features.CreateProduct;

namespace Okomos.Inventory.Tests.Features;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_Sku_Is_Empty()
    {
        var result = await _validator.ValidateAsync(new CreateProductCommand("Name", "", 1, 10m));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Pass_When_Valid()
    {
        var result = await _validator.ValidateAsync(new CreateProductCommand("Name", "SKU-1", 1, 10m));

        result.IsValid.Should().BeTrue();
    }
}
