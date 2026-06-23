using Okomos.Accounting.Persistence;
using Okomos.Accounting.Persistence.Entities;
using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Okomos.Accounting.Features.CreateJournalEntry;

public sealed class CreateJournalEntryCommandHandler : ICommandHandler<CreateJournalEntryCommand, Guid>
{
    private readonly AccountingDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CreateJournalEntryCommandHandler(AccountingDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public Task<Guid> HandleAsync(CreateJournalEntryCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.CurrentTenantId
            ?? throw new InvalidOperationException("Tenant is required.");

        var entry = JournalEntry.Create(tenantId, command.Description, command.Debit, command.Credit);
        _dbContext.JournalEntries.Add(entry);

        return Task.FromResult(entry.Id);
    }
}
