using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Testcontainers.MsSql;

namespace Identity.Tests.Infrastructure;

public sealed class IdentitySqlServerFixture : IAsyncLifetime
{
    private MsSqlContainer? _container;

    public bool IsReady { get; private set; }
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        if (!DockerHelper.IsAvailable)
            return;

        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
        IsReady = true;

        await using var scope = await CreateScopeAsync();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await context.Database.EnsureCreatedAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = "User",
                NormalizedName = "USER",
                TenantId = Guid.Empty
            });
        }
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }

    public Task<AsyncServiceScope> CreateScopeAsync(Guid? tenantId = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(ConnectionString));

        services.AddScoped<ITenantProvider>(_ => new TestTenantProvider(tenantId ?? Guid.NewGuid()));

        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        var provider = services.BuildServiceProvider();
        return Task.FromResult(provider.CreateAsyncScope());
    }
}

[CollectionDefinition(nameof(IdentitySqlServerCollection))]
public sealed class IdentitySqlServerCollection : ICollectionFixture<IdentitySqlServerFixture>;
