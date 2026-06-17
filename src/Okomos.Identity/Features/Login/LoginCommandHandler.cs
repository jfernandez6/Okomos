using Okomos.Identity.Persistence.Entities;
using Okomos.Identity.Services;
using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.AspNetCore.Identity;

namespace Okomos.Identity.Features.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            throw new SharedKernel.Exceptions.ValidationException(
                new Dictionary<string, string[]> { ["Email"] = ["Invalid credentials."] });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new SharedKernel.Exceptions.ValidationException(
                new Dictionary<string, string[]> { ["Email"] = ["Invalid credentials."] });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var tokenResult = _jwtTokenService.GenerateTokens(user, roles, claims);

        user.RefreshToken = tokenResult.RefreshToken;
        user.RefreshTokenExpiry = tokenResult.RefreshTokenExpiry;
        await _userManager.UpdateAsync(user);

        return new LoginResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.ExpiresAt,
            user.Id,
            user.Email!);
    }
}
