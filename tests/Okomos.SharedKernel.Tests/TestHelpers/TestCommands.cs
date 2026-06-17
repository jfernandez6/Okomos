using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.SharedKernel.Tests.TestHelpers;

public sealed record TestCommand(string Value) : ICommand<string>;

public sealed record TestQuery(Guid Id) : IQuery<string?>;

public sealed class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    public Task<string> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult($"handled:{command.Value}");
}

public sealed class TestQueryHandler : IQueryHandler<TestQuery, string?>
{
    public Task<string?> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(query.Id == Guid.Empty ? null : $"entity:{query.Id}");
}
