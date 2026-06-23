using Okomos.Inventory.EventHandlers;
using Okomos.Inventory.Features.CreateProduct;
using Okomos.Inventory.Features.GetProductById;
using Okomos.Inventory.Persistence;
using Okomos.SharedKernel;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Behaviors.Validation;
using Okomos.SharedKernel.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Okomos.Inventory;

public static class InventoryDependencyInjection
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InventoryConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Inventory connection string is not configured.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "inventory")));

        services.AddOutboxStore<InventoryDbContext>();

        services.AddScoped<IDomainEventHandler<ProductCreatedEvent>, ProductCreatedDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<JournalEntryCreatedIntegrationEvent>, JournalEntryCreatedIntegrationEventHandler>();

        services.AddScoped<IValidator<CreateProductCommand>, CreateProductCommandValidator>();
        services.AddCommandHandler<CreateProductCommand, Guid, CreateProductCommandHandler, InventoryDbContext>();

        services.AddQueryHandler<GetProductByIdQuery, ProductDto?, GetProductByIdQueryHandler>();

        return services;
    }
}
