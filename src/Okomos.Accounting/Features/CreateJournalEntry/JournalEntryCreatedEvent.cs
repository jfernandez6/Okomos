using Okomos.SharedKernel.Abstractions.Events;

namespace Okomos.Accounting.Features.CreateJournalEntry;

public sealed record JournalEntryCreatedEvent(Guid EntryId, Guid TenantId, decimal Debit, decimal Credit) : DomainEvent;
