using FluentAssertions;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Outbox;
using Okomos.SharedKernel.Events;
using Okomos.SharedKernel.IntegrationEvents;
using Okomos.SharedKernel.Outbox;
using Okomos.SharedKernel.Persistence;
using Okomos.SharedKernel.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Testcontainers.MsSql;

namespace Okomos.SharedKernel.Tests.Outbox;

public class OutboxIntegrationTests
{
    [DockerFact]
    public async Task OutboxProcessor_Should_Process_Message_From_SqlServer()
    {
        await using var container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        await container.StartAsync();

        var connectionString = container.GetConnectionString();
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<OutboxStore<TestDbContext>>();
        services.AddScoped<IOutboxStore<TestDbContext>>(sp => sp.GetRequiredService<OutboxStore<TestDbContext>>());
        services.AddSingleton(new OutboxStoreRegistration(typeof(TestDbContext)));

        var integrationHandler = Substitute.For<IIntegrationEventHandler<InvoiceCreatedIntegrationEvent>>();
        services.AddSingleton(integrationHandler);
        services.AddSingleton<IEventBus, EventBus>();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore<TestDbContext>>();
        var invoiceId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var integrationEvent = new InvoiceCreatedIntegrationEvent(invoiceId, tenantId, 999.99m);

        await outboxStore.AddAsync(integrationEvent, tenantId);

        var processor = new OutboxProcessorHostedService(
            provider,
            scope.ServiceProvider.GetRequiredService<ILogger<OutboxProcessorHostedService>>());

        await InvokeProcessOutbox(processor);

        await integrationHandler.Received(1).HandleAsync(
            Arg.Is<InvoiceCreatedIntegrationEvent>(e => e.InvoiceId == invoiceId),
            Arg.Any<CancellationToken>());

        var message = await dbContext.OutboxMessages.SingleAsync();
        message.ProcessedOn.Should().NotBeNull();
    }

    private static async Task InvokeProcessOutbox(OutboxProcessorHostedService processor)
    {
        var method = typeof(OutboxProcessorHostedService)
            .GetMethod("ProcessOutboxMessagesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        await (Task)method!.Invoke(processor, [CancellationToken.None])!;
    }
}
