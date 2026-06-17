using Okomos.Billing.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Okomos.Billing.Tests.Infrastructure;

public sealed class BillingSqlServerFixture : IAsyncLifetime
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

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        var tenantProvider = new TestTenantProvider(Guid.NewGuid());
        var eventBus = new TestEventBus();

        await using var context = new BillingDbContext(options, tenantProvider, eventBus);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}

[CollectionDefinition(nameof(BillingSqlServerCollection))]
public sealed class BillingSqlServerCollection : ICollectionFixture<BillingSqlServerFixture>;
