using Okomos.SharedKernel.Behaviors.Validation;

namespace Okomos.Accounting.Features.CreateJournalEntry;

public sealed class CreateJournalEntryCommandValidator : IValidator<CreateJournalEntryCommand>
{
    public Task<ValidationResult> ValidateAsync(CreateJournalEntryCommand request, CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(request.Description))
            result.AddError(nameof(request.Description), "Description is required.");

        if (request.Debit != request.Credit)
            result.AddError(nameof(request.Credit), "Debit and Credit must be equal.");

        return Task.FromResult(result);
    }
}
