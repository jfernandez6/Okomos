using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Accounting.Features.CreateJournalEntry;

public sealed record CreateJournalEntryCommand(
    string Description,
    decimal Debit,
    decimal Credit) : ICommand<Guid>;
