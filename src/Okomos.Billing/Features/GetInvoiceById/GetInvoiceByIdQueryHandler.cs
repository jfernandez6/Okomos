using Okomos.Billing.Persistence;
using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Billing.Features.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler : IQueryHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly BillingDbContext _dbContext;

    public GetInvoiceByIdQueryHandler(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InvoiceDto?> HandleAsync(GetInvoiceByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.Id == query.InvoiceId)
            .Select(i => new InvoiceDto(
                i.Id,
                i.TenantId,
                i.CustomerName,
                i.Amount,
                i.Currency,
                i.IssuedAt,
                i.Status))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
