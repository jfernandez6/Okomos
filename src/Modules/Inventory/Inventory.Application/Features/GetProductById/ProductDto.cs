namespace Inventory.Application.Features.GetProductById;

public sealed record ProductDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Sku,
    int QuantityOnHand,
    decimal UnitPrice);
