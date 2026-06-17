namespace Okomos.SharedKernel.Abstractions.Multitenancy;

public interface ITenantProvider
{
    Guid? CurrentTenantId { get; }
    string? CurrentTenantSlug { get; }
    string? GetConnectionString();
    void SetTenant(Guid tenantId, string? tenantSlug = null);
}
