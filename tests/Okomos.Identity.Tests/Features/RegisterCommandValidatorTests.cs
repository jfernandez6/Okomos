using FluentAssertions;
using Okomos.Identity.Features.Register;

namespace Okomos.Identity.Tests.Features;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_Email_Is_Empty()
    {
        var result = await _validator.ValidateAsync(
            new RegisterCommand("", "Password123!", "John", "Doe"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey(nameof(RegisterCommand.Email));
    }

    [Fact]
    public async Task Should_Fail_When_Password_Is_Too_Short()
    {
        var result = await _validator.ValidateAsync(
            new RegisterCommand("user@test.com", "short", "John", "Doe"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey(nameof(RegisterCommand.Password));
    }

    [Fact]
    public async Task Should_Pass_When_Valid()
    {
        var result = await _validator.ValidateAsync(
            new RegisterCommand("user@test.com", "Password123!", "John", "Doe"));

        result.IsValid.Should().BeTrue();
    }
}
