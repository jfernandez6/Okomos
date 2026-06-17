using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Events;

namespace Okomos.SharedKernel.Behaviors.DomainEvents;

public sealed class DomainEventsCommandDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public DomainEventsCommandDecorator(
        ICommandHandler<TCommand, TResult> inner,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _inner = inner;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _inner.HandleAsync(command, cancellationToken);
        await _domainEventDispatcher.DispatchPendingEventsAsync(cancellationToken);
        return result;
    }
}
