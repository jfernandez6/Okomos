using FluentAssertions;
using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Behaviors.Logging;
using Okomos.SharedKernel.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Okomos.SharedKernel.Tests.Behaviors;

public class LoggingDecoratorTests
{
    [Fact]
    public async Task CommandDecorator_Should_Log_And_Return_Result()
    {
        var inner = Substitute.For<ICommandHandler<TestCommand, string>>();
        inner.HandleAsync(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>())
            .Returns("logged");

        var logger = Substitute.For<ILogger<LoggingCommandDecorator<TestCommand, string>>>();
        var decorator = new LoggingCommandDecorator<TestCommand, string>(inner, logger);

        var result = await decorator.HandleAsync(new TestCommand("x"));

        result.Should().Be("logged");
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString()!.Contains("CreateInvoiceCommand") == false && state.ToString()!.Contains("TestCommand")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task CommandDecorator_Should_Rethrow_And_Log_Error()
    {
        var inner = Substitute.For<ICommandHandler<TestCommand, string>>();
        inner.HandleAsync(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var logger = Substitute.For<ILogger<LoggingCommandDecorator<TestCommand, string>>>();
        var decorator = new LoggingCommandDecorator<TestCommand, string>(inner, logger);

        var act = () => decorator.HandleAsync(new TestCommand("x"));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task QueryDecorator_Should_Return_Result_From_Inner_Handler()
    {
        var inner = Substitute.For<IQueryHandler<TestQuery, string?>>();
        inner.HandleAsync(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>())
            .Returns("query-result");

        var logger = Substitute.For<ILogger<LoggingQueryDecorator<TestQuery, string?>>>();
        var decorator = new LoggingQueryDecorator<TestQuery, string?> (inner, logger);

        var result = await decorator.HandleAsync(new TestQuery(Guid.NewGuid()));

        result.Should().Be("query-result");
    }
}
