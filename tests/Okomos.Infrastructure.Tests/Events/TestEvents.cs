using Okomos.SharedKernel.Abstractions.Events;

namespace Okomos.Infrastructure.Tests.Events;

public sealed record TestDomainEvent : DomainEvent;

public sealed record TestIntegrationEvent : IntegrationEvent;
