using Accounting.Application.Features.CreateJournalEntry;
using Accounting.Application.Features.GetJournalEntryById;
using Accounting.Domain.DomainEvents;
using Accounting.Infrastructure.EventHandlers;
using Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Okomos.Infrastructure;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.IntegrationEvents;

namespace Accounting.Module;

public static class AccountingModule
{
    public static IServiceCollection AddAccountingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AccountingConnection")
            ?? throw new InvalidOperationException("Accounting connection string is not configured.");

        services.AddDbContext<AccountingDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddOutboxStore<AccountingDbContext>();
        services.AddUnitOfWork<AccountingDbContext>();

        services.AddScoped<CreateJournalEntryHandler>();
        services.AddScoped<GetJournalEntryByIdHandler>();

        services.AddScoped<IDomainEventHandler<JournalEntryCreatedEvent>, JournalEntryCreatedDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<InvoiceCreatedIntegrationEvent>, InvoiceCreatedIntegrationEventHandler>();

        return services;
    }
}
