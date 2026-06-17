using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Inventory.Features.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDto?>;

public sealed record ProductDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Sku,
    int QuantityOnHand,
    decimal UnitPrice);
