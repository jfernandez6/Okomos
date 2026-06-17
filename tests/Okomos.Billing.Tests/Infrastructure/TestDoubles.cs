using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Okomos.Billing.Tests.Infrastructure;

public sealed class TestTenantProvider : ITenantProvider
{
    public TestTenantProvider(Guid tenantId, string? slug = null)
    {
        SetTenant(tenantId, slug);
    }

    public Guid? CurrentTenantId { get; private set; }
    public string? CurrentTenantSlug { get; private set; }

    public string? GetConnectionString() => null;

    public void SetTenant(Guid tenantId, string? tenantSlug = null)
    {
        CurrentTenantId = tenantId;
        CurrentTenantSlug = tenantSlug;
    }
}

public sealed class TestEventBus : IEventBus
{
    public List<IDomainEvent> DomainEvents { get; } = [];
    public List<IIntegrationEvent> IntegrationEvents { get; } = [];

    public Task PublishDomainEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        DomainEvents.Add(domainEvent);
        return Task.CompletedTask;
    }

    public Task PublishIntegrationEventAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        IntegrationEvents.Add(integrationEvent);
        return Task.CompletedTask;
    }
}
