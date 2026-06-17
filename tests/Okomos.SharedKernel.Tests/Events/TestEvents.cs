using Okomos.SharedKernel.Abstractions.Events;

namespace Okomos.SharedKernel.Tests.Events;

public sealed record TestDomainEvent : DomainEvent;

public sealed record TestIntegrationEvent : IntegrationEvent;
