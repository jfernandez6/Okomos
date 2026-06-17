using FluentAssertions;
using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Behaviors.Validation;
using Okomos.SharedKernel.Exceptions;
using Okomos.SharedKernel.Tests.TestHelpers;
using NSubstitute;

namespace Okomos.SharedKernel.Tests.Behaviors;

public class ValidationDecoratorTests
{
    [Fact]
    public async Task CommandDecorator_Should_Call_Inner_Handler_When_Valid()
    {
        var inner = Substitute.For<ICommandHandler<TestCommand, string>>();
        inner.HandleAsync(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>())
            .Returns("ok");

        var validator = Substitute.For<IValidator<TestCommand>>();
        validator.ValidateAsync(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Success());

        var decorator = new ValidationCommandDecorator<TestCommand, string>(inner, [validator]);

        var result = await decorator.HandleAsync(new TestCommand("test"));

        result.Should().Be("ok");
        await inner.Received(1).HandleAsync(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CommandDecorator_Should_Throw_ValidationException_When_Invalid()
    {
        var inner = Substitute.For<ICommandHandler<TestCommand, string>>();
        var validator = Substitute.For<IValidator<TestCommand>>();
        var validationResult = ValidationResult.Success();
        validationResult.AddError("Value", "Value is required.");
        validator.ValidateAsync(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);

        var decorator = new ValidationCommandDecorator<TestCommand, string>(inner, [validator]);

        var act = () => decorator.HandleAsync(new TestCommand(""));

        await act.Should().ThrowAsync<ValidationException>();
        await inner.DidNotReceive().HandleAsync(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryDecorator_Should_Call_Inner_Handler_When_Valid()
    {
        var inner = Substitute.For<IQueryHandler<TestQuery, string?>>();
        inner.HandleAsync(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>())
            .Returns("found");

        var validator = Substitute.For<IValidator<TestQuery>>();
        validator.ValidateAsync(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Success());

        var decorator = new ValidationQueryDecorator<TestQuery, string?>(inner, [validator]);
        var result = await decorator.HandleAsync(new TestQuery(Guid.NewGuid()));

        result.Should().Be("found");
    }
}
