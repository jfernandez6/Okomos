using Identity.Domain.Entities;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Okomos.SharedKernel.Exceptions;

namespace Identity.Application.Features.Refresh;

public sealed class RefreshHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<RefreshResponse> Handle(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u =>
                u.RefreshToken == request.RefreshToken &&
                u.RefreshTokenExpiry > DateTime.UtcNow,
                cancellationToken);

        if (user is null)
        {
            throw new ValidationException(
                new Dictionary<string, string[]> { ["RefreshToken"] = ["Invalid or expired refresh token."] });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var tokenResult = _jwtTokenService.GenerateTokens(user, roles, claims);

        user.RefreshToken = tokenResult.RefreshToken;
        user.RefreshTokenExpiry = tokenResult.RefreshTokenExpiry;
        await _userManager.UpdateAsync(user);

        return new RefreshResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.ExpiresAt);
    }
}
