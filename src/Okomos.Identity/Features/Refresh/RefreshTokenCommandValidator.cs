using FastEndpoints;

namespace Okomos.Identity.Features.Refresh;

public sealed class RefreshTokenCommandValidator : Validator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}
