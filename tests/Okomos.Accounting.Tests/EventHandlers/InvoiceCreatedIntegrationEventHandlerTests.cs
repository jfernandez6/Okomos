using FluentAssertions;
using Okomos.Accounting.EventHandlers;
using Okomos.SharedKernel.IntegrationEvents;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Okomos.Accounting.Tests.EventHandlers;

public class InvoiceCreatedIntegrationEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Process_InvoiceCreated_Event()
    {
        var logger = Substitute.For<ILogger<InvoiceCreatedIntegrationEventHandler>>();
        var handler = new InvoiceCreatedIntegrationEventHandler(logger);
        var integrationEvent = new InvoiceCreatedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), 300m);

        await handler.HandleAsync(integrationEvent);

        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Accounting received InvoiceCreated")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
