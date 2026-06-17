using FluentAssertions;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Okomos.SharedKernel.Tests.Events;

public class EventBusTests
{
    [Fact]
    public async Task Should_Dispatch_Domain_Event_To_Registered_Handler()
    {
        var handler = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        var services = new ServiceCollection();
        services.AddSingleton(handler);
        var provider = services.BuildServiceProvider();

        var logger = Substitute.For<ILogger<EventBus>>();
        var eventBus = new EventBus(provider, logger);

        var domainEvent = new TestDomainEvent();
        await eventBus.PublishDomainEventAsync(domainEvent);

        await handler.Received(1).HandleAsync(domainEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Dispatch_Integration_Event_To_Registered_Handler()
    {
        var handler = Substitute.For<IIntegrationEventHandler<TestIntegrationEvent>>();
        var services = new ServiceCollection();
        services.AddSingleton(handler);
        var provider = services.BuildServiceProvider();

        var logger = Substitute.For<ILogger<EventBus>>();
        var eventBus = new EventBus(provider, logger);

        var integrationEvent = new TestIntegrationEvent();
        await eventBus.PublishIntegrationEventAsync(integrationEvent);

        await handler.Received(1).HandleAsync(integrationEvent, Arg.Any<CancellationToken>());
    }
}
