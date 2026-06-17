using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Okomos.SharedKernel.Behaviors.Multitenancy;

public sealed class MultitenancyCommandDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly ITenantProvider _tenantProvider;

    public MultitenancyCommandDecorator(
        ICommandHandler<TCommand, TResult> inner,
        ITenantProvider tenantProvider)
    {
        _inner = inner;
        _tenantProvider = tenantProvider;
    }

    public Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        if (_tenantProvider.CurrentTenantId is null)
        {
            throw new InvalidOperationException(
                $"Tenant context is required to execute command '{typeof(TCommand).Name}'.");
        }

        return _inner.HandleAsync(command, cancellationToken);
    }
}

public sealed class MultitenancyQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _inner;
    private readonly ITenantProvider _tenantProvider;

    public MultitenancyQueryDecorator(
        IQueryHandler<TQuery, TResult> inner,
        ITenantProvider tenantProvider)
    {
        _inner = inner;
        _tenantProvider = tenantProvider;
    }

    public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        if (_tenantProvider.CurrentTenantId is null)
        {
            throw new InvalidOperationException(
                $"Tenant context is required to execute query '{typeof(TQuery).Name}'.");
        }

        return _inner.HandleAsync(query, cancellationToken);
    }
}
