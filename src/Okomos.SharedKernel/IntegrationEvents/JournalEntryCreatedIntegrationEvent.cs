using Okomos.SharedKernel.Abstractions.Events;

namespace Okomos.SharedKernel.IntegrationEvents;

public sealed record JournalEntryCreatedIntegrationEvent(
    Guid EntryId,
    Guid TenantId,
    decimal Debit,
    decimal Credit) : IntegrationEvent;
