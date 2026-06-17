namespace Okomos.SharedKernel.Abstractions.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
