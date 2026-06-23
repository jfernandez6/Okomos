using Okomos.SharedKernel.Abstractions.Events;

namespace Accounting.Domain.DomainEvents;

public sealed record JournalEntryCreatedEvent(Guid EntryId, Guid TenantId, decimal Debit, decimal Credit) : DomainEvent;
