using Okomos.Billing.EventHandlers;
using Okomos.Billing.Features.CreateInvoice;
using Okomos.Billing.Features.GetInvoiceById;
using Okomos.Billing.Persistence;
using Okomos.SharedKernel;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Okomos.Billing;

public static class BillingDependencyInjection
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BillingConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Billing connection string is not configured.");

        services.AddDbContext<BillingDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "billing")));

        services.AddOutboxStore<BillingDbContext>();

        services.AddScoped<IDomainEventHandler<InvoiceCreatedEvent>, InvoiceCreatedDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ProductCreatedIntegrationEvent>, ProductCreatedIntegrationEventHandler>();

        services.AddCommandHandler<CreateInvoiceCommand, Guid, CreateInvoiceCommandHandler, BillingDbContext>();

        services.AddQueryHandler<GetInvoiceByIdQuery, InvoiceDto?, GetInvoiceByIdQueryHandler>();

        return services;
    }
}
