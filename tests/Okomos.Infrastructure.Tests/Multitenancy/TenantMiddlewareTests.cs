using Okomos.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Okomos.Infrastructure.Tests.Multitenancy;

public class TenantMiddlewareTests
{
    [Fact]
    public async Task Should_Set_Tenant_From_X_Tenant_Id_Header()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TenantProvider(new ConfigurationBuilder().Build());
        var middleware = new TenantMiddleware(_ => Task.CompletedTask);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-Id"] = tenantId.ToString();
        context.Request.Headers["X-Tenant-Slug"] = "acme";

        await middleware.InvokeAsync(context, tenantProvider);

        tenantProvider.CurrentTenantId.Should().Be(tenantId);
        tenantProvider.CurrentTenantSlug.Should().Be("acme");
    }

    [Fact]
    public async Task Should_Set_Tenant_From_Claim()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TenantProvider(new ConfigurationBuilder().Build());
        var middleware = new TenantMiddleware(_ => Task.CompletedTask);

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("tenant_id", tenantId.ToString())
        ]));

        await middleware.InvokeAsync(context, tenantProvider);

        tenantProvider.CurrentTenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task Should_Not_Set_Tenant_When_No_Context_Provided()
    {
        var tenantProvider = new TenantProvider(new ConfigurationBuilder().Build());
        var middleware = new TenantMiddleware(_ => Task.CompletedTask);

        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, tenantProvider);

        tenantProvider.CurrentTenantId.Should().BeNull();
    }
}
