using Okomos.Accounting.Features.CreateJournalEntry;
using Okomos.SharedKernel.Abstractions.Entities;

namespace Okomos.Accounting.Persistence.Entities;

public sealed class JournalEntry : Entity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public DateTime EntryDate { get; set; }

    public static JournalEntry Create(Guid tenantId, string description, decimal debit, decimal credit)
    {
        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Description = description,
            Debit = debit,
            Credit = credit,
            EntryDate = DateTime.UtcNow
        };

        entry.AddDomainEvent(new JournalEntryCreatedEvent(entry.Id, entry.TenantId, entry.Debit, entry.Credit));
        return entry;
    }
}
