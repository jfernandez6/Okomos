using Okomos.SharedKernel.Abstractions.Events;
using Okomos.SharedKernel.Abstractions.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Okomos.Infrastructure.Persistence;

public sealed class OutboxStore<TDbContext> : IOutboxStore<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public OutboxStore(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(IIntegrationEvent integrationEvent, Guid? tenantId, CancellationToken cancellationToken = default)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = integrationEvent.EventType,
            Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
            OccurredOn = integrationEvent.OccurredOn,
            TenantId = tenantId
        };

        _dbContext.Set<OutboxMessage>().Add(message);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null && m.Error == null)
            .OrderBy(m => m.OccurredOn)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.Set<OutboxMessage>().FindAsync([messageId], cancellationToken);
        if (message is not null)
        {
            message.ProcessedOn = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.Set<OutboxMessage>().FindAsync([messageId], cancellationToken);
        if (message is not null)
        {
            message.Error = error;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
