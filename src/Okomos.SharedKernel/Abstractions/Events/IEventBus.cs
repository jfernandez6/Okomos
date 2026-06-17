namespace Okomos.SharedKernel.Abstractions.Events;

public interface IEventBus
{
    Task PublishDomainEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task PublishIntegrationEventAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
