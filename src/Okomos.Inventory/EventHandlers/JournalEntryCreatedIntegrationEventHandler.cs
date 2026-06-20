using Microsoft.Extensions.Logging;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.IntegrationEvents;

namespace Okomos.Inventory.EventHandlers;

public sealed class JournalEntryCreatedIntegrationEventHandler : IIntegrationEventHandler<JournalEntryCreatedIntegrationEvent>
{
    private readonly ILogger<JournalEntryCreatedIntegrationEventHandler> _logger;

    public JournalEntryCreatedIntegrationEventHandler(ILogger<JournalEntryCreatedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(JournalEntryCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Inventory received JournalEntryCreated for Entry {EntryId} with Debit {Debit} and Credit {Credit}",
            integrationEvent.EntryId,
            integrationEvent.Debit,
            integrationEvent.Credit);

        return Task.CompletedTask;
    }
}
