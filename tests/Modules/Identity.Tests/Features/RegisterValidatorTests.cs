using Identity.Application.Features.Register;

namespace Identity.Tests.Features;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_Email_Is_Empty()
    {
        var result = await _validator.ValidateAsync(
            new RegisterRequest("", "Password123!", "John", "Doe"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Fact]
    public async Task Should_Fail_When_Password_Is_Too_Short()
    {
        var result = await _validator.ValidateAsync(
            new RegisterRequest("user@test.com", "short", "John", "Doe"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public async Task Should_Pass_When_Valid()
    {
        var result = await _validator.ValidateAsync(
            new RegisterRequest("user@test.com", "Password123!", "John", "Doe"));

        result.IsValid.Should().BeTrue();
    }
}
