using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Okomos.Infrastructure.Persistence;

public sealed class DesignTimeTenantProvider : ITenantProvider
{
    public Guid? CurrentTenantId => null;
    public string? CurrentTenantSlug => null;

    public string? GetConnectionString() => null;

    public void SetTenant(Guid tenantId, string? tenantSlug = null)
    {
    }
}
