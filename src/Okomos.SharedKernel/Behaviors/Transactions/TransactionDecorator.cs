using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.EntityFrameworkCore;

namespace Okomos.SharedKernel.Behaviors.Transactions;

public sealed class TransactionCommandDecorator<TCommand, TResult, TDbContext> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TDbContext : DbContext
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly TDbContext _dbContext;

    public TransactionCommandDecorator(
        ICommandHandler<TCommand, TResult> inner,
        TDbContext dbContext)
    {
        _inner = inner;
        _dbContext = dbContext;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await _inner.HandleAsync(command, cancellationToken);
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
