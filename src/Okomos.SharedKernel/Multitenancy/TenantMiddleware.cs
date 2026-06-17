using Okomos.SharedKernel.Abstractions.Multitenancy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Okomos.SharedKernel.Multitenancy;

public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        var tenantId = ResolveTenantId(context);

        if (tenantId.HasValue)
        {
            var tenantSlug = context.Request.Headers["X-Tenant-Slug"].FirstOrDefault();
            tenantProvider.SetTenant(tenantId.Value, tenantSlug);
        }

        await _next(context);
    }

    private static Guid? ResolveTenantId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue)
            && Guid.TryParse(headerValue.FirstOrDefault(), out var headerTenantId))
        {
            return headerTenantId;
        }

        var claimValue = context.User.FindFirstValue("tenant_id");
        if (!string.IsNullOrWhiteSpace(claimValue) && Guid.TryParse(claimValue, out var claimTenantId))
        {
            return claimTenantId;
        }

        return null;
    }
}
