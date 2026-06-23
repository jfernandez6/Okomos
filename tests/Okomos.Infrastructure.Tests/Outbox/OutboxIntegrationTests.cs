using Okomos.Infrastructure.Events;
using Okomos.Infrastructure.Outbox;
using Okomos.Infrastructure.Persistence;
using Okomos.Infrastructure.Tests.TestHelpers;
using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Outbox;
using Okomos.SharedKernel.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.MsSql;

namespace Okomos.Infrastructure.Tests.Outbox;

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

        services.Configure<OutboxOptions>(_ => { });

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore<TestDbContext>>();
        var invoiceId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var integrationEvent = new InvoiceCreatedIntegrationEvent(invoiceId, tenantId, 999.99m);

        await outboxStore.AddAsync(integrationEvent, tenantId);
        await dbContext.SaveChangesAsync();

        var processor = new OutboxProcessorHostedService(
            provider,
            provider.GetRequiredService<ILogger<OutboxProcessorHostedService>>(),
            provider.GetRequiredService<IOptions<OutboxOptions>>());

        await processor.ProcessOutboxMessagesAsync(CancellationToken.None);

        await integrationHandler.Received(1).HandleAsync(
            Arg.Is<InvoiceCreatedIntegrationEvent>(e => e.InvoiceId == invoiceId),
            Arg.Any<CancellationToken>());

        var message = await dbContext.OutboxMessages.SingleAsync();
        message.ProcessedOn.Should().NotBeNull();
    }
}
