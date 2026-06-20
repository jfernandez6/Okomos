using Microsoft.Extensions.Logging;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.IntegrationEvents;

namespace Okomos.Billing.EventHandlers;

public sealed class ProductCreatedIntegrationEventHandler : IIntegrationEventHandler<ProductCreatedIntegrationEvent>
{
    private readonly ILogger<ProductCreatedIntegrationEventHandler> _logger;

    public ProductCreatedIntegrationEventHandler(ILogger<ProductCreatedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(ProductCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Billing received ProductCreated for Product {ProductId} with SKU {Sku}",
            integrationEvent.ProductId,
            integrationEvent.Sku);

        return Task.CompletedTask;
    }
}
