using FluentAssertions;
using Okomos.SharedKernel.Behaviors.DomainEvents;
using Okomos.SharedKernel.Events;
using Okomos.SharedKernel.Tests.TestHelpers;
using NSubstitute;

namespace Okomos.SharedKernel.Tests.Behaviors;

public class DomainEventsDecoratorTests
{
    [Fact]
    public async Task Should_Dispatch_Pending_Events_After_Handler_Completes()
    {
        var inner = new TestCommandHandler();
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        var decorator = new DomainEventsCommandDecorator<TestCommand, string>(inner, dispatcher);

        var result = await decorator.HandleAsync(new TestCommand("events"));

        result.Should().Be("handled:events");
        await dispatcher.Received(1).DispatchPendingEventsAsync(Arg.Any<CancellationToken>());
    }
}
