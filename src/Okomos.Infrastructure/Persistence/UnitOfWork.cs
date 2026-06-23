using Okomos.Infrastructure.Events;
using Okomos.SharedKernel.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Okomos.Infrastructure.Persistence;

public interface IUnitOfWork<TDbContext> where TDbContext : DbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly DomainEventDispatcher _domainEventDispatcher;

    public UnitOfWork(
        TDbContext dbContext,
        IEventBus eventBus,
        ILogger<DomainEventDispatcher> logger)
    {
        _dbContext = dbContext;
        _domainEventDispatcher = new DomainEventDispatcher(dbContext, eventBus, logger);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _domainEventDispatcher.DispatchPendingEventsAsync(cancellationToken);
                var result = await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
