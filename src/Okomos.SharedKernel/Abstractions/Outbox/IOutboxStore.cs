using Okomos.SharedKernel.Abstractions.Events;
using Microsoft.EntityFrameworkCore;

namespace Okomos.SharedKernel.Abstractions.Outbox;

public interface IOutboxStore
{
    Task AddAsync(IIntegrationEvent integrationEvent, Guid? tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
}

public interface IOutboxStore<TDbContext> : IOutboxStore where TDbContext : DbContext;

