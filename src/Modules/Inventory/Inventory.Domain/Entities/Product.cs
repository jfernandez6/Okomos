using Inventory.Domain.DomainEvents;
using Okomos.SharedKernel.Abstractions.Entities;

namespace Inventory.Domain.Entities;

public sealed class Product : Entity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public decimal UnitPrice { get; set; }

    public static Product Create(Guid tenantId, string name, string sku, int quantity, decimal unitPrice)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Sku = sku,
            QuantityOnHand = quantity,
            UnitPrice = unitPrice
        };

        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.TenantId, product.Sku));
        return product;
    }
}
