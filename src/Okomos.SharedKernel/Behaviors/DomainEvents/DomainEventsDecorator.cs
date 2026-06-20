using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Events;
using Microsoft.EntityFrameworkCore;

namespace Okomos.SharedKernel.Behaviors.DomainEvents;

public sealed class DomainEventsCommandDecorator<TCommand, TResult, TDbContext> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TDbContext : DbContext
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly TDbContext _dbContext;

    public DomainEventsCommandDecorator(
        ICommandHandler<TCommand, TResult> inner,
        IDomainEventDispatcher domainEventDispatcher,
        TDbContext dbContext)
    {
        _inner = inner;
        _domainEventDispatcher = domainEventDispatcher;
        _dbContext = dbContext;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _inner.HandleAsync(command, cancellationToken);
        await _domainEventDispatcher.DispatchPendingEventsAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
