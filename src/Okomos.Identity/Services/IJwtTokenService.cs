using Okomos.Identity.Persistence.Entities;
using System.Security.Claims;

namespace Okomos.Identity.Services;

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
