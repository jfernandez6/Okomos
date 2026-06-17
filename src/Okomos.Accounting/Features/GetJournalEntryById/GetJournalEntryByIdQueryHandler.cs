using Okomos.Accounting.Persistence;
using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Accounting.Features.GetJournalEntryById;

public sealed class GetJournalEntryByIdQueryHandler : IQueryHandler<GetJournalEntryByIdQuery, JournalEntryDto?>
{
    private readonly AccountingDbContext _dbContext;

    public GetJournalEntryByIdQueryHandler(AccountingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<JournalEntryDto?> HandleAsync(GetJournalEntryByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.Id == query.EntryId)
            .Select(e => new JournalEntryDto(
                e.Id,
                e.TenantId,
                e.Description,
                e.Debit,
                e.Credit,
                e.EntryDate))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
