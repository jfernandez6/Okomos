using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.Extensions.Logging;

namespace Okomos.SharedKernel.Behaviors.Logging;

public sealed class LoggingCommandDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly ILogger<LoggingCommandDecorator<TCommand, TResult>> _logger;

    public LoggingCommandDecorator(
        ICommandHandler<TCommand, TResult> inner,
        ILogger<LoggingCommandDecorator<TCommand, TResult>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var commandName = typeof(TCommand).Name;
        _logger.LogInformation("Handling command {CommandName}", commandName);

        try
        {
            var result = await _inner.HandleAsync(command, cancellationToken);
            _logger.LogInformation("Command {CommandName} handled successfully", commandName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command {CommandName}", commandName);
            throw;
        }
    }
}

public sealed class LoggingQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _inner;
    private readonly ILogger<LoggingQueryDecorator<TQuery, TResult>> _logger;

    public LoggingQueryDecorator(
        IQueryHandler<TQuery, TResult> inner,
        ILogger<LoggingQueryDecorator<TQuery, TResult>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        var queryName = typeof(TQuery).Name;
        _logger.LogInformation("Handling query {QueryName}", queryName);

        try
        {
            var result = await _inner.HandleAsync(query, cancellationToken);
            _logger.LogInformation("Query {QueryName} handled successfully", queryName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling query {QueryName}", queryName);
            throw;
        }
    }
}
