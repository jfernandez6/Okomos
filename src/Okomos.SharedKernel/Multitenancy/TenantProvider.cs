using Okomos.SharedKernel.Abstractions.Multitenancy;
using Microsoft.Extensions.Configuration;

namespace Okomos.SharedKernel.Multitenancy;

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
        if (_tenantSlug is not null)
        {
            var tenantConnection = _configuration.GetConnectionString($"Tenant_{_tenantSlug}");
            if (!string.IsNullOrWhiteSpace(tenantConnection))
            {
                return tenantConnection;
            }
        }

        return _configuration.GetConnectionString("DefaultConnection");
    }
}
