using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Accounting.Features.GetJournalEntryById;

public sealed record GetJournalEntryByIdQuery(Guid EntryId) : IQuery<JournalEntryDto?>;

public sealed record JournalEntryDto(
    Guid Id,
    Guid TenantId,
    string Description,
    decimal Debit,
    decimal Credit,
    DateTime EntryDate);
