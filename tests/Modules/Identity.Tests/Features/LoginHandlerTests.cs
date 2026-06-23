using Identity.Application.Features.Login;
using Identity.Domain.Entities;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Okomos.SharedKernel.Exceptions;

namespace Identity.Tests.Features;

public class LoginHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _signInManager = Substitute.For<SignInManager<ApplicationUser>>(
            _userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null);

        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _handler = new LoginHandler(_userManager, _signInManager, _jwtTokenService);
    }

    [Fact]
    public async Task Handle_Should_Return_Tokens_When_Credentials_Are_Valid()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test@test.com" };
        _userManager.FindByEmailAsync("test@test.com").Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, "Password123!", true)
            .Returns(SignInResult.Success);
        _userManager.GetRolesAsync(user).Returns(["User"]);
        _userManager.GetClaimsAsync(user).Returns([]);
        _jwtTokenService.GenerateTokens(user, Arg.Any<IList<string>>(), Arg.Any<IList<System.Security.Claims.Claim>>())
            .Returns(new TokenResult("access-token", "refresh-token", DateTime.UtcNow.AddMinutes(60), DateTime.UtcNow.AddDays(7)));

        var request = new LoginRequest("test@test.com", "Password123!");

        var result = await _handler.Handle(request);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be("test@test.com");
        await _userManager.Received(1).UpdateAsync(user);
    }

    [Fact]
    public async Task Handle_Should_Throw_ValidationException_When_User_Not_Found()
    {
        _userManager.FindByEmailAsync("test@test.com").Returns((ApplicationUser?)null);

        var request = new LoginRequest("test@test.com", "Password123!");

        var act = () => _handler.Handle(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_Should_Throw_ValidationException_When_Password_Is_Invalid()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test@test.com" };
        _userManager.FindByEmailAsync("test@test.com").Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, "WrongPassword!", true)
            .Returns(SignInResult.Failed);

        var request = new LoginRequest("test@test.com", "WrongPassword!");

        var act = () => _handler.Handle(request);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
