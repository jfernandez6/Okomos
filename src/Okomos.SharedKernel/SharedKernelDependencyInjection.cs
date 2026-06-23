using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Behaviors.DomainEvents;
using Okomos.SharedKernel.Behaviors.Logging;
using Okomos.SharedKernel.Behaviors.Multitenancy;
using Okomos.SharedKernel.Behaviors.Transactions;
using Okomos.SharedKernel.Behaviors.Validation;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Abstractions.Outbox;
using Okomos.SharedKernel.Events;
using Okomos.SharedKernel.Multitenancy;
using Okomos.SharedKernel.Outbox;
using Okomos.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Okomos.SharedKernel;

public static class SharedKernelDependencyInjection
{
    public static IServiceCollection AddSharedKernel(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<IEventBus, EventBus>();
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
        services.AddHostedService<OutboxProcessorHostedService>();

        return services;
    }

    public static IServiceCollection AddCommandHandler<TCommand, TResult, THandler, TDbContext>(
        this IServiceCollection services,
        bool useTransaction = true,
        bool useMultitenancy = true,
        bool useDomainEvents = true)
        where TCommand : ICommand<TResult>
        where THandler : class, ICommandHandler<TCommand, TResult>
        where TDbContext : DbContext
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResult>>(sp =>
        {
            ICommandHandler<TCommand, TResult> handler = sp.GetRequiredService<THandler>();

            if (useDomainEvents)
            {
                var dbContext = sp.GetRequiredService<TDbContext>();
                handler = new DomainEventsCommandDecorator<TCommand, TResult, TDbContext>(
                    handler,
                    new DomainEventDispatcher(
                        dbContext,
                        sp.GetRequiredService<IEventBus>(),
                        sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DomainEventDispatcher>>()),
                    dbContext);
            }

            if (useTransaction)
            {
                handler = new TransactionCommandDecorator<TCommand, TResult, TDbContext>(
                    handler,
                    sp.GetRequiredService<TDbContext>());
            }

            if (useMultitenancy)
            {
                handler = new MultitenancyCommandDecorator<TCommand, TResult>(
                    handler,
                    sp.GetRequiredService<ITenantProvider>());
            }

            handler = new LoggingCommandDecorator<TCommand, TResult>(
                handler,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingCommandDecorator<TCommand, TResult>>>());

            handler = new ValidationCommandDecorator<TCommand, TResult>(
                handler,
                sp.GetServices<IValidator<TCommand>>());

            return handler;
        });

        return services;
    }

    public static IServiceCollection AddQueryHandler<TQuery, TResult, THandler>(
        this IServiceCollection services,
        bool useMultitenancy = true)
        where TQuery : IQuery<TResult>
        where THandler : class, IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResult>>(sp =>
        {
            IQueryHandler<TQuery, TResult> handler = sp.GetRequiredService<THandler>();

            if (useMultitenancy)
            {
                handler = new MultitenancyQueryDecorator<TQuery, TResult>(
                    handler,
                    sp.GetRequiredService<ITenantProvider>());
            }

            handler = new LoggingQueryDecorator<TQuery, TResult>(
                handler,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingQueryDecorator<TQuery, TResult>>>());

            handler = new ValidationQueryDecorator<TQuery, TResult>(
                handler,
                sp.GetServices<IValidator<TQuery>>());

            return handler;
        });

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
}
