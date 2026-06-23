using Identity.Application.Features.Login;

namespace Identity.Tests.Features;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_Password_Is_Empty()
    {
        var result = await _validator.ValidateAsync(new LoginRequest("user@test.com", ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginRequest.Password));
    }

    [Fact]
    public async Task Should_Pass_When_Valid()
    {
        var result = await _validator.ValidateAsync(new LoginRequest("user@test.com", "Password123!"));

        result.IsValid.Should().BeTrue();
    }
}
