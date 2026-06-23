using Billing.Domain.Entities;
using Billing.Infrastructure.Persistence;
using Okomos.Infrastructure.Persistence;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Billing.Application.Features.CreateInvoice;

public sealed class CreateInvoiceHandler
{
    private readonly BillingDbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly IUnitOfWork<BillingDbContext> _unitOfWork;

    public CreateInvoiceHandler(
        BillingDbContext db,
        ITenantProvider tenant,
        IUnitOfWork<BillingDbContext> unitOfWork)
    {
        _db = db;
        _tenant = tenant;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateInvoiceResponse> Handle(CreateInvoiceRequest req, CancellationToken ct)
    {
        var tenantId = _tenant.CurrentTenantId ?? throw new InvalidOperationException("Tenant is required.");
        var invoice = Invoice.Create(tenantId, req.CustomerName, req.Amount, req.Currency);
        _db.Invoices.Add(invoice);
        await _unitOfWork.SaveChangesAsync(ct);
        return new CreateInvoiceResponse(invoice.Id);
    }
}
