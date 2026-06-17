using Okomos.SharedKernel.Abstractions.Entities;
using Okomos.SharedKernel.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Okomos.SharedKernel.Events;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly DbContext _dbContext;
    private readonly IEventBus _eventBus;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        DbContext dbContext,
        IEventBus eventBus,
        ILogger<DomainEventDispatcher> logger)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task DispatchPendingEventsAsync(CancellationToken cancellationToken = default)
    {
        var entities = _dbContext.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            _logger.LogDebug("Dispatching pending domain event {EventType}", domainEvent.GetType().Name);
            await _eventBus.PublishDomainEventAsync(domainEvent, cancellationToken);
        }
    }
}
