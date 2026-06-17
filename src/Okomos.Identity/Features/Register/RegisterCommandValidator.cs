using Okomos.SharedKernel.Behaviors.Validation;

namespace Okomos.Identity.Features.Register;

public sealed class RegisterCommandValidator : IValidator<RegisterCommand>
{
    public Task<ValidationResult> ValidateAsync(RegisterCommand request, CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(request.Email))
            result.AddError(nameof(request.Email), "Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            result.AddError(nameof(request.Password), "Password must be at least 8 characters.");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            result.AddError(nameof(request.FirstName), "First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            result.AddError(nameof(request.LastName), "Last name is required.");

        return Task.FromResult(result);
    }
}
