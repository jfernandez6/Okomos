using Billing.Application.Features.CreateInvoice;

namespace Billing.Tests.Features;

public class CreateInvoiceValidatorTests
{
    private readonly CreateInvoiceValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_CustomerName_Is_Empty()
    {
        var result = await _validator.ValidateAsync(new CreateInvoiceRequest("", 100m, "MXN"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceRequest.CustomerName));
    }

    [Fact]
    public async Task Should_Fail_When_Amount_Is_Zero()
    {
        var result = await _validator.ValidateAsync(new CreateInvoiceRequest("Customer", 0m, "MXN"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceRequest.Amount));
    }

    [Fact]
    public async Task Should_Pass_When_Request_Is_Valid()
    {
        var result = await _validator.ValidateAsync(new CreateInvoiceRequest("Customer", 100m, "MXN"));

        result.IsValid.Should().BeTrue();
    }
}
