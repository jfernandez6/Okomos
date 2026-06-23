namespace Okomos.Infrastructure.Events;

public interface IDomainEventDispatcher
{
    Task DispatchPendingEventsAsync(CancellationToken cancellationToken = default);
}
