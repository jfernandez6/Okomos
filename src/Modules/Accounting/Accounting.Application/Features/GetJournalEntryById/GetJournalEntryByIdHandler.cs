using Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Features.GetJournalEntryById;

public sealed class GetJournalEntryByIdHandler
{
    private readonly AccountingDbContext _db;

    public GetJournalEntryByIdHandler(AccountingDbContext db)
    {
        _db = db;
    }

    public async Task<JournalEntryDto?> Handle(GetJournalEntryByIdRequest req, CancellationToken ct)
    {
        return await _db.JournalEntries
            .AsNoTracking()
            .Where(e => e.Id == req.EntryId)
            .Select(e => new JournalEntryDto(
                e.Id,
                e.TenantId,
                e.Description,
                e.Debit,
                e.Credit,
                e.EntryDate))
            .FirstOrDefaultAsync(ct);
    }
}
