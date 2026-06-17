using Okomos.SharedKernel.Behaviors.Validation;

namespace Okomos.Identity.Features.Login;

public sealed class LoginCommandValidator : IValidator<LoginCommand>
{
    public Task<ValidationResult> ValidateAsync(LoginCommand request, CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(request.Email))
            result.AddError(nameof(request.Email), "Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            result.AddError(nameof(request.Password), "Password is required.");

        return Task.FromResult(result);
    }
}
