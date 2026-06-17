using Okomos.Accounting.EventHandlers;
using Okomos.Accounting.Features.CreateJournalEntry;
using Okomos.Accounting.Features.GetJournalEntryById;
using Okomos.Accounting.Persistence;
using Okomos.SharedKernel;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Behaviors.Validation;
using Okomos.SharedKernel.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Okomos.Accounting;

public static class AccountingDependencyInjection
{
    public static IServiceCollection AddAccountingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AccountingConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Accounting connection string is not configured.");

        services.AddDbContext<AccountingDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "accounting")));

        services.AddOutboxStore<AccountingDbContext>();
        services.AddDomainEventDispatcher<AccountingDbContext>();

        services.AddScoped<IIntegrationEventHandler<InvoiceCreatedIntegrationEvent>, InvoiceCreatedIntegrationEventHandler>();

        services.AddScoped<IValidator<CreateJournalEntryCommand>, CreateJournalEntryCommandValidator>();
        services.AddCommandHandler<CreateJournalEntryCommand, Guid, CreateJournalEntryCommandHandler, AccountingDbContext>();

        services.AddQueryHandler<GetJournalEntryByIdQuery, JournalEntryDto?, GetJournalEntryByIdQueryHandler>();

        return services;
    }
}
