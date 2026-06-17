using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Exceptions;

namespace Okomos.SharedKernel.Behaviors.Validation;

public sealed class ValidationCommandDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    public ValidationCommandDecorator(
        ICommandHandler<TCommand, TResult> inner,
        IEnumerable<IValidator<TCommand>> validators)
    {
        _inner = inner;
        _validators = validators;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(command, cancellationToken);
            foreach (var error in result.Errors)
            {
                if (errors.TryGetValue(error.Key, out var existing))
                {
                    errors[error.Key] = [.. existing, .. error.Value];
                }
                else
                {
                    errors[error.Key] = error.Value;
                }
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        return await _inner.HandleAsync(command, cancellationToken);
    }
}

public sealed class ValidationQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _inner;
    private readonly IEnumerable<IValidator<TQuery>> _validators;

    public ValidationQueryDecorator(
        IQueryHandler<TQuery, TResult> inner,
        IEnumerable<IValidator<TQuery>> validators)
    {
        _inner = inner;
        _validators = validators;
    }

    public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(query, cancellationToken);
            foreach (var error in result.Errors)
            {
                if (errors.TryGetValue(error.Key, out var existing))
                {
                    errors[error.Key] = [.. existing, .. error.Value];
                }
                else
                {
                    errors[error.Key] = error.Value;
                }
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        return await _inner.HandleAsync(query, cancellationToken);
    }
}
