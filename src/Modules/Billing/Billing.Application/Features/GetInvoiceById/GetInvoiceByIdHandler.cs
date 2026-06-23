using Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Features.GetInvoiceById;

public sealed class GetInvoiceByIdHandler
{
    private readonly BillingDbContext _db;

    public GetInvoiceByIdHandler(BillingDbContext db)
    {
        _db = db;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdRequest req, CancellationToken ct)
    {
        return await _db.Invoices
            .AsNoTracking()
            .Where(i => i.Id == req.InvoiceId)
            .Select(i => new InvoiceDto(
                i.Id,
                i.TenantId,
                i.CustomerName,
                i.Amount,
                i.Currency,
                i.IssuedAt,
                i.Status))
            .FirstOrDefaultAsync(ct);
    }
}
