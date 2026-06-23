using Billing.Domain.DomainEvents;
using Okomos.SharedKernel.Abstractions.Entities;

namespace Billing.Domain.Entities;

public sealed class Invoice : Entity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime IssuedAt { get; set; }
    public string Status { get; set; } = "Draft";

    public static Invoice Create(Guid tenantId, string customerName, decimal amount, string currency)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerName = customerName,
            Amount = amount,
            Currency = currency,
            IssuedAt = DateTime.UtcNow,
            Status = "Issued"
        };

        invoice.AddDomainEvent(new InvoiceCreatedEvent(invoice.Id, invoice.TenantId, invoice.Amount));
        return invoice;
    }
}
