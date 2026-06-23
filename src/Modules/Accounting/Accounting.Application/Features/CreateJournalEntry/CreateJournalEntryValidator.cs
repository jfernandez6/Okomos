using FastEndpoints;
using FluentValidation;

namespace Accounting.Application.Features.CreateJournalEntry;

public sealed class CreateJournalEntryValidator : Validator<CreateJournalEntryRequest>
{
    public CreateJournalEntryValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(x => x.Credit)
            .Equal(x => x.Debit)
            .WithMessage("Debit and Credit must be equal.");
    }
}
