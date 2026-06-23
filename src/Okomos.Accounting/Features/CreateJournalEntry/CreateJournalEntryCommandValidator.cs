using FastEndpoints;

namespace Okomos.Accounting.Features.CreateJournalEntry;

public sealed class CreateJournalEntryCommandValidator : Validator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(x => x.Credit)
            .Equal(x => x.Debit)
            .WithMessage("Debit and Credit must be equal.");
    }
}
