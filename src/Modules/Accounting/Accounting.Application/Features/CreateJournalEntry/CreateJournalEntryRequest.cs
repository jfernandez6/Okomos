namespace Accounting.Application.Features.CreateJournalEntry;

public sealed record CreateJournalEntryRequest(string Description, decimal Debit, decimal Credit);
