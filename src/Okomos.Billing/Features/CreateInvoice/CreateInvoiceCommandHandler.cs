using Okomos.Billing.Persistence;
using Okomos.Billing.Persistence.Entities;
using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Okomos.Billing.Features.CreateInvoice;

public sealed class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, Guid>
{
    private readonly BillingDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CreateInvoiceCommandHandler(BillingDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> HandleAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.CurrentTenantId
            ?? throw new InvalidOperationException("Tenant is required.");

        var invoice = Invoice.Create(tenantId, command.CustomerName, command.Amount, command.Currency);
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return invoice.Id;
    }
}
