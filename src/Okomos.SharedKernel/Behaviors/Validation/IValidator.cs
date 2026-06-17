namespace Okomos.SharedKernel.Behaviors.Validation;

public interface IValidator<in TRequest>
{
    Task<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}

public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public IDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>();

    public static ValidationResult Success() => new();

    public void AddError(string propertyName, string error)
    {
        if (Errors.TryGetValue(propertyName, out var existing))
        {
            Errors[propertyName] = [.. existing, error];
        }
        else
        {
            Errors[propertyName] = [error];
        }
    }
}
