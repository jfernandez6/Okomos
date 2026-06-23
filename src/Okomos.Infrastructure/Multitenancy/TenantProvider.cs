using Okomos.SharedKernel.Abstractions.Multitenancy;
using Microsoft.Extensions.Configuration;

namespace Okomos.Infrastructure.Multitenancy;

public sealed class TenantProvider : ITenantProvider
{
    private readonly IConfiguration _configuration;
    private Guid? _tenantId;
    private string? _tenantSlug;

    public TenantProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Guid? CurrentTenantId => _tenantId;
    public string? CurrentTenantSlug => _tenantSlug;

    public void SetTenant(Guid tenantId, string? tenantSlug = null)
    {
        _tenantId = tenantId;
        _tenantSlug = tenantSlug;
    }

    public string? GetConnectionString()
    {
        if (_tenantSlug is null)
        {
            return null;
        }

        return _configuration.GetConnectionString($"Tenant_{_tenantSlug}");
    }
}
