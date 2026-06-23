using Inventory.Application.Features.CreateProduct;
using Inventory.Application.Features.GetProductById;
using Inventory.Domain.DomainEvents;
using Inventory.Infrastructure.EventHandlers;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Okomos.Infrastructure;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.IntegrationEvents;

namespace Inventory.Module;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InventoryConnection")
            ?? throw new InvalidOperationException("Inventory connection string is not configured.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddOutboxStore<InventoryDbContext>();
        services.AddUnitOfWork<InventoryDbContext>();

        services.AddScoped<CreateProductHandler>();
        services.AddScoped<GetProductByIdHandler>();

        services.AddScoped<IDomainEventHandler<ProductCreatedEvent>, ProductCreatedDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<JournalEntryCreatedIntegrationEvent>, JournalEntryCreatedIntegrationEventHandler>();

        return services;
    }
}
