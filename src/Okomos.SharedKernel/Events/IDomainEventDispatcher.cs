using Okomos.SharedKernel.Abstractions.Events;

namespace Okomos.SharedKernel.Events;

public interface IDomainEventDispatcher
{
    Task DispatchPendingEventsAsync(CancellationToken cancellationToken = default);
}
