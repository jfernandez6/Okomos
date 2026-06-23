namespace Accounting.Application.Features.GetJournalEntryById;

public sealed record JournalEntryDto(
    Guid Id,
    Guid TenantId,
    string Description,
    decimal Debit,
    decimal Credit,
    DateTime EntryDate);
