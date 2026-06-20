using Okomos.Accounting.Features.CreateJournalEntry;
using Okomos.Accounting.Persistence;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Outbox;
using Okomos.SharedKernel.IntegrationEvents;

namespace Okomos.Accounting.EventHandlers;

public sealed class JournalEntryCreatedDomainEventHandler : IDomainEventHandler<JournalEntryCreatedEvent>
{
    private readonly IOutboxStore<AccountingDbContext> _outboxStore;

    public JournalEntryCreatedDomainEventHandler(IOutboxStore<AccountingDbContext> outboxStore)
    {
        _outboxStore = outboxStore;
    }

    public async Task HandleAsync(JournalEntryCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var integrationEvent = new JournalEntryCreatedIntegrationEvent(
            domainEvent.EntryId,
            domainEvent.TenantId,
            domainEvent.Debit,
            domainEvent.Credit);

        await _outboxStore.AddAsync(integrationEvent, domainEvent.TenantId, cancellationToken);
    }
}
