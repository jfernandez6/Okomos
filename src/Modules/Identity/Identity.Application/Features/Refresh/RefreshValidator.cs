using FastEndpoints;
using FluentValidation;

namespace Identity.Application.Features.Refresh;

public sealed class RefreshValidator : Validator<RefreshRequest>
{
    public RefreshValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}
