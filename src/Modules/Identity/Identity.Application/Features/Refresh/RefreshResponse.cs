namespace Identity.Application.Features.Refresh;

public sealed record RefreshResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
