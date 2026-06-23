using FluentAssertions;
using Okomos.Billing.Features.CreateInvoice;

namespace Okomos.Billing.Tests.Features;

public class CreateInvoiceCommandValidatorTests
{
    private readonly CreateInvoiceCommandValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_CustomerName_Is_Empty()
    {
        var result = await _validator.ValidateAsync(new CreateInvoiceCommand("", 100m, "MXN"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceCommand.CustomerName));
    }

    [Fact]
    public async Task Should_Fail_When_Amount_Is_Zero()
    {
        var result = await _validator.ValidateAsync(new CreateInvoiceCommand("Customer", 0m, "MXN"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateInvoiceCommand.Amount));
    }

    [Fact]
    public async Task Should_Pass_When_Command_Is_Valid()
    {
        var result = await _validator.ValidateAsync(new CreateInvoiceCommand("Customer", 100m, "MXN"));

        result.IsValid.Should().BeTrue();
    }
}
