using Okomos.Infrastructure.Events;
using Okomos.Infrastructure.Multitenancy;
using Okomos.Infrastructure.Outbox;
using Okomos.Infrastructure.Persistence;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Abstractions.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Okomos.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<IEventBus, EventBus>();
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
        services.AddHostedService<OutboxProcessorHostedService>();

        return services;
    }

    public static IServiceCollection AddOutboxStore<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<OutboxStore<TDbContext>>();
        services.AddScoped<IOutboxStore<TDbContext>>(sp => sp.GetRequiredService<OutboxStore<TDbContext>>());
        services.AddSingleton(new OutboxStoreRegistration(typeof(TDbContext)));
        return services;
    }

    public static IServiceCollection AddUnitOfWork<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IUnitOfWork<TDbContext>, UnitOfWork<TDbContext>>();
        return services;
    }
}
