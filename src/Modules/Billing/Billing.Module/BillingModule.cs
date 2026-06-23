using Billing.Application.Features.CreateInvoice;
using Billing.Application.Features.GetInvoiceById;
using Billing.Domain.DomainEvents;
using Billing.Infrastructure.EventHandlers;
using Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Okomos.Infrastructure;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.IntegrationEvents;

namespace Billing.Module;

public static class BillingModule
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BillingConnection")
            ?? throw new InvalidOperationException("Billing connection string is not configured.");

        services.AddDbContext<BillingDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddOutboxStore<BillingDbContext>();
        services.AddUnitOfWork<BillingDbContext>();

        services.AddScoped<CreateInvoiceHandler>();
        services.AddScoped<GetInvoiceByIdHandler>();

        services.AddScoped<IDomainEventHandler<InvoiceCreatedEvent>, InvoiceCreatedDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ProductCreatedIntegrationEvent>, ProductCreatedIntegrationEventHandler>();

        return services;
    }
}
