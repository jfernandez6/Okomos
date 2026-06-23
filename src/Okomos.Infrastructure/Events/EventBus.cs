using Okomos.SharedKernel.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Okomos.Infrastructure.Events;

public sealed class EventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBus> _logger;
    private readonly ConcurrentDictionary<Type, DomainEventInvoker> _domainEventInvokers = new();
    private readonly ConcurrentDictionary<Type, IntegrationEventInvoker> _integrationEventInvokers = new();

    public EventBus(IServiceProvider serviceProvider, ILogger<EventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishDomainEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        var invoker = _domainEventInvokers.GetOrAdd(eventType, CreateDomainEventInvoker);

        _logger.LogInformation("Dispatching domain event {EventType}", eventType.Name);
        await invoker(_serviceProvider, domainEvent, cancellationToken);
    }

    public async Task PublishIntegrationEventAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var eventType = integrationEvent.GetType();
        var invoker = _integrationEventInvokers.GetOrAdd(eventType, CreateIntegrationEventInvoker);

        _logger.LogInformation("Dispatching integration event {EventType}", eventType.Name);
        await invoker(_serviceProvider, integrationEvent, cancellationToken);
    }

    private static DomainEventInvoker CreateDomainEventInvoker(Type eventType)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handleAsyncMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

        return async (sp, evt, ct) =>
        {
            var handlers = sp.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                await (Task)handleAsyncMethod.Invoke(handler, [evt, ct])!;
            }
        };
    }

    private static IntegrationEventInvoker CreateIntegrationEventInvoker(Type eventType)
    {
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handleAsyncMethod = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync))!;

        return async (sp, evt, ct) =>
        {
            var handlers = sp.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                await (Task)handleAsyncMethod.Invoke(handler, [evt, ct])!;
            }
        };
    }
}

internal delegate Task DomainEventInvoker(IServiceProvider serviceProvider, object domainEvent, CancellationToken cancellationToken);
internal delegate Task IntegrationEventInvoker(IServiceProvider serviceProvider, object integrationEvent, CancellationToken cancellationToken);
