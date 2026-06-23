using Microsoft.Extensions.Logging;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.IntegrationEvents;

namespace Accounting.Infrastructure.EventHandlers;

public sealed class InvoiceCreatedIntegrationEventHandler : IIntegrationEventHandler<InvoiceCreatedIntegrationEvent>
{
    private readonly ILogger<InvoiceCreatedIntegrationEventHandler> _logger;

    public InvoiceCreatedIntegrationEventHandler(ILogger<InvoiceCreatedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(InvoiceCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Accounting received InvoiceCreated for Invoice {InvoiceId} with Amount {Amount}",
            integrationEvent.InvoiceId,
            integrationEvent.Amount);

        return Task.CompletedTask;
    }
}
