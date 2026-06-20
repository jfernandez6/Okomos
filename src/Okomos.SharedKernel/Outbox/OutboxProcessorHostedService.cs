using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Okomos.SharedKernel.Outbox;

public sealed class OutboxProcessorHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorHostedService> _logger;
    private readonly OutboxOptions _options;

    public OutboxProcessorHostedService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorHostedService> logger,
        IOptions<OutboxOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started (polling interval: {Interval}s)", _options.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
        }
    }

    internal async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var registrations = scope.ServiceProvider.GetServices<OutboxStoreRegistration>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        foreach (var registration in registrations)
        {
            var storeType = typeof(IOutboxStore<>).MakeGenericType(registration.DbContextType);
            var outboxStore = (IOutboxStore)scope.ServiceProvider.GetRequiredService(storeType);
            var messages = await outboxStore.GetPendingAsync(_options.BatchSize, cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    var eventType = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t => t.Name == message.EventType && typeof(IIntegrationEvent).IsAssignableFrom(t));

                    if (eventType is null)
                    {
                        await outboxStore.MarkAsFailedAsync(message.Id, $"Event type '{message.EventType}' not found.", cancellationToken);
                        continue;
                    }

                    var integrationEvent = (IIntegrationEvent)JsonSerializer.Deserialize(message.Payload, eventType)!;
                    _logger.LogInformation(
                        "Processing outbox message {MessageId} ({EventType}) from {DbContext}",
                        message.Id,
                        message.EventType,
                        registration.DbContextType.Name);

                    await eventBus.PublishIntegrationEventAsync(integrationEvent, cancellationToken);
                    await outboxStore.MarkAsProcessedAsync(message.Id, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
                    await outboxStore.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
                }
            }
        }
    }
}

public sealed class OutboxOptions
{
    public int PollingIntervalSeconds { get; set; } = 10;
    public int BatchSize { get; set; } = 20;
}
