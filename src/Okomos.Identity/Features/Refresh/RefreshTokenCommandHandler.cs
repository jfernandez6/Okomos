using Okomos.Identity.Persistence.Entities;
using Okomos.Identity.Services;
using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Identity.Features.Refresh;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<RefreshTokenResponse> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u =>
                u.RefreshToken == command.RefreshToken &&
                u.RefreshTokenExpiry > DateTime.UtcNow,
                cancellationToken);

        if (user is null)
        {
            throw new SharedKernel.Exceptions.ValidationException(
                new Dictionary<string, string[]> { ["RefreshToken"] = ["Invalid or expired refresh token."] });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var tokenResult = _jwtTokenService.GenerateTokens(user, roles, claims);

        user.RefreshToken = tokenResult.RefreshToken;
        user.RefreshTokenExpiry = tokenResult.RefreshTokenExpiry;
        await _userManager.UpdateAsync(user);

        return new RefreshTokenResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.ExpiresAt);
    }
}
