using Accounting.Domain.Entities;
using Accounting.Infrastructure.Persistence;
using Okomos.Infrastructure.Persistence;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Accounting.Application.Features.CreateJournalEntry;

public sealed class CreateJournalEntryHandler
{
    private readonly AccountingDbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly IUnitOfWork<AccountingDbContext> _unitOfWork;

    public CreateJournalEntryHandler(
        AccountingDbContext db,
        ITenantProvider tenant,
        IUnitOfWork<AccountingDbContext> unitOfWork)
    {
        _db = db;
        _tenant = tenant;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateJournalEntryResponse> Handle(CreateJournalEntryRequest req, CancellationToken ct)
    {
        var tenantId = _tenant.CurrentTenantId ?? throw new InvalidOperationException("Tenant is required.");
        var entry = JournalEntry.Create(tenantId, req.Description, req.Debit, req.Credit);
        _db.JournalEntries.Add(entry);
        await _unitOfWork.SaveChangesAsync(ct);
        return new CreateJournalEntryResponse(entry.Id);
    }
}
