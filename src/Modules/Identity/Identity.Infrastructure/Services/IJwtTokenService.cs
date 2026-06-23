using Identity.Domain.Entities;
using System.Security.Claims;

namespace Identity.Infrastructure.Services;

public sealed record TokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    DateTime RefreshTokenExpiry);

public interface IJwtTokenService
{
    TokenResult GenerateTokens(
        ApplicationUser user,
        IList<string> roles,
        IList<Claim> claims);
}
