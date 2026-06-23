using FluentAssertions;
using Okomos.Identity.Features.Login;

namespace Okomos.Identity.Tests.Features;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_Password_Is_Empty()
    {
        var result = await _validator.ValidateAsync(new LoginCommand("user@test.com", ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Fact]
    public async Task Should_Pass_When_Valid()
    {
        var result = await _validator.ValidateAsync(new LoginCommand("user@test.com", "Password123!"));

        result.IsValid.Should().BeTrue();
    }
}
